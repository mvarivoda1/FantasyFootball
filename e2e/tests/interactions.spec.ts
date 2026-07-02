import type { Page } from '@playwright/test';
import { test, expect, gotoOk, anonymousState } from './helpers';

/**
 * Interakcijski testovi — stvarno KORISTE aplikaciju, ne provjeravaju samo
 * render: slaganje početnog sastava (two-click swap), pravi transferi kroz
 * košaricu i modal, kreiranje liga + pridruživanje šifrom, te puni put novog
 * korisnika (registracija → build momčadi → spremanje sastava).
 *
 * Testovi demo računa mijenjaju zajedničko stanje (sastav, budžet, lige) pa se
 * vrte SERIJSKI i svaki vraća stanje kakvo je zatekao. Uz ostatak suite-a se u
 * pravilu vrte bez problema; za potpuno determinističko izvođenje:
 * `npm test -- --workers=1`.
 */

const pitchPlayer = (page: Page, id: string) =>
  page.locator(`[data-role="pitch"] .ff-pitch-player[data-player-id="${id}"]`);
const benchPlayer = (page: Page, id: string) =>
  page.locator(`[data-role="bench"] .ff-pitch-player[data-player-id="${id}"]`);

/**
 * Two-click swap na MyTeam: prvi igrač s klupe ↔ starter iste pozicije
 * (uvijek legalan swap). Vraća oba id-a za kasniju provjeru/vraćanje.
 */
async function swapFirstBenchIntoLineup(page: Page): Promise<{ benchId: string; starterId: string }> {
  const benchTile = page.locator('[data-role="bench"] .ff-pitch-player').first();
  const benchId = (await benchTile.getAttribute('data-player-id'))!;
  const pos = await benchTile.getAttribute('data-position');

  const starterTile = page.locator(`[data-role="pitch"] .ff-pitch-player[data-position="${pos}"]`).first();
  const starterId = (await starterTile.getAttribute('data-player-id'))!;

  await benchTile.click();
  await expect(benchTile).toHaveClass(/is-selected/);
  await starterTile.click();

  // Swap se odmah vidi u DOM-u: klupaš je na terenu, starter na klupi.
  await expect(pitchPlayer(page, benchId)).toBeVisible();
  await expect(benchPlayer(page, starterId)).toBeVisible();
  return { benchId, starterId };
}

/**
 * Spremi sastav i dočekaj potvrdu. Postojeći toast se prvo makne da nova
 * potvrda dokazuje NOVI (post-redirect) render, a ne ostatak starog DOM-a.
 */
async function saveLineupAndExpectConfirmation(page: Page) {
  const toast = page.locator('.ff-myteam-toast--success');
  if ((await toast.count()) > 0) {
    await toast.locator('[data-dismiss-toast]').click();
    await expect(toast).toHaveCount(0);
  }
  await page.locator('[data-role="save-lineup"]').click();
  await expect(page.locator('.ff-myteam-toast--success')).toContainText('uspješno spremljen');
}

/** Na transfer terenu označi igrača za prodaju (postane prazno "+" mjesto). */
async function markForSale(page: Page, playerId: string) {
  const tile = page.locator(`.ff-transfer__pitch-col .ff-pitch-player[data-player-id="${playerId}"]`);
  await tile.click();
  await expect(tile).toHaveClass(/ff-pitch-player--selling/);
}

/**
 * Kupi najjeftinijeg kandidata zadane pozicije koji prolazi pravila. Lista je
 * sortirana po cijeni; odbijeni klik (budžet / klupski limit) prepozna se po
 * izostanku `is-selected` pa se ide na sljedećeg. Vraća id kupljenog igrača.
 */
async function buyCheapestAcceptedCandidate(page: Page, posVar: string): Promise<string> {
  const candidates = page.locator(`[data-role="market-card"][data-player-pos-var="${posVar}"]:not([disabled])`);
  const total = Math.min(await candidates.count(), 15);
  for (let i = 0; i < total; i++) {
    const card = candidates.nth(i);
    await card.click();
    if (/\bis-selected\b/.test((await card.getAttribute('class')) ?? '')) {
      return (await card.getAttribute('data-player-id'))!;
    }
  }
  throw new Error(`Nijedan od ${total} kandidata pozicije ${posVar} nije prošao pravila (budžet / klupski limit).`);
}

/**
 * Potvrdi košaricu kroz modal i dočekaj poruku uspjeha. Postojeći alert se
 * prvo makne da nova poruka dokazuje NOVI (post-redirect) render.
 */
async function confirmTransfers(page: Page) {
  const alert = page.locator('.ff-transfer-alert--success');
  if ((await alert.count()) > 0) {
    await alert.locator('[data-dismiss-alert]').click();
    await expect(alert).toHaveCount(0);
  }
  const backdrop = page.locator('[data-role="confirm-backdrop"]');
  await page.locator('[data-role="cart-confirm"]').click();
  await expect(backdrop).toHaveClass(/is-open/);
  await backdrop.locator('[data-role="confirm-submit"]').click();
  await expect(page.locator('.ff-transfer-alert--success')).toContainText('Transfer dovršen: 1 OUT, 1 IN');
}

// ---- Demo račun: sastav, transferi, lige (dijele stanje → serijski) ----

test.describe.serial('interakcije demo računa', () => {

  test('MyTeam: igrač s klupe ulazi u početni sastav, sprema se i vraća', async ({ page }) => {
    await gotoOk(page, '/FantasyTeam/MyTeam');

    // Ubaci klupaša u početni sastav i spremi
    const { benchId, starterId } = await swapFirstBenchIntoLineup(page);
    await saveLineupAndExpectConfirmation(page);

    // Nakon redirecta je swap perzistiran (server ga je stvarno spremio)
    await expect(pitchPlayer(page, benchId)).toBeVisible();
    await expect(benchPlayer(page, starterId)).toBeVisible();

    // Vrati originalni sastav i spremi — test je idempotentan
    await benchPlayer(page, starterId).click();
    await pitchPlayer(page, benchId).click();
    await expect(pitchPlayer(page, starterId)).toBeVisible();
    await expect(benchPlayer(page, benchId)).toBeVisible();

    await saveLineupAndExpectConfirmation(page);
    await expect(pitchPlayer(page, starterId)).toBeVisible();
    await expect(benchPlayer(page, benchId)).toBeVisible();
  });

  test('Transferi: prodaja + kupnja kroz košaricu i modal, pa obrnuti transfer', async ({ page }) => {
    // Prodajemo igrača s KLUPE — prodaja startera resetira spremljeni sastav.
    await gotoOk(page, '/FantasyTeam/MyTeam');
    const benchTile = page.locator('[data-role="bench"] .ff-pitch-player').first();
    const outId = (await benchTile.getAttribute('data-player-id'))!;
    const posVar = ((await benchTile.getAttribute('data-position')) ?? '').toLowerCase();

    await gotoOk(page, '/transferi');
    const section = page.locator('section.ff-transfer');
    const seasonStarted = (await section.getAttribute('data-season-started')) === 'true';
    const freeTransfers = Number((await section.getAttribute('data-free-transfers')) ?? '0');
    // Test radi 2 transfera (tamo i natrag); bez 2 besplatna bi demo timu
    // trajno skinuo bodove (−4 po transferu preko kvote).
    test.skip(seasonStarted && freeTransfers < 2,
      `Samo ${freeTransfers} besplatnih transfera — preskačem da ne skidam bodove demo timu.`);

    const budgetBefore = (await page.locator('[data-role="budget-display"]').innerText()).trim();

    // OUT + IN + potvrda kroz modal; novi igrač na terenu, prodani opet dostupan
    await markForSale(page, outId);
    const inId = await buyCheapestAcceptedCandidate(page, posVar);
    await confirmTransfers(page);
    await expect(page.locator(`.ff-transfer__pitch-col .ff-pitch-player[data-player-id="${inId}"]`)).toBeVisible();
    const outCard = page.locator(`[data-role="market-card"][data-player-id="${outId}"]`);
    await expect(outCard).toBeEnabled();

    // Obrnuti transfer → vraća originalni sastav i budžet
    await markForSale(page, inId);
    await outCard.click();
    await expect(outCard).toHaveClass(/is-selected/);
    await confirmTransfers(page);

    await expect(page.locator(`.ff-transfer__pitch-col .ff-pitch-player[data-player-id="${outId}"]`)).toBeVisible();
    await expect(page.locator('[data-role="budget-display"]')).toHaveText(budgetBefore);
  });

  test('Liga: kreiraj dvije lige pa se šifrom pridruži prvoj', async ({ page }) => {
    const stamp = Date.now();

    // Ime tima — da ga kasnije nađemo u poretku lige. textContent (ne
    // innerText) jer je naslov CSS-om uppercase-an, a tablica lige nije.
    await gotoOk(page, '/FantasyTeam/MyTeam');
    const teamName = ((await page.locator('.ff-myteam__title').textContent()) ?? '').trim();
    expect(teamName.length).toBeGreaterThan(0);

    const createLeague = async (name: string) => {
      await gotoOk(page, '/League/Create');
      await page.locator('#Name').fill(name);
      await page.locator('#MaxTeams').fill('10');
      await page.getByRole('button', { name: 'Kreiraj ligu', exact: true }).click();
      await expect(page).toHaveURL(/\/League\/Created\/\d+/);
      const id = Number(page.url().match(/\/(\d+)(?:[/?#]|$)/)![1]);
      const code = (await page.locator('#copyJoinCodeBtn').getAttribute('data-code')) ?? '';
      return { id, code };
    };

    // Liga A: sa Created stranice uzimamo join šifru
    const leagueA = await createLeague(`E2E Join Liga A ${stamp}`);
    expect(leagueA.code).toMatch(/^[A-Z0-9]{6}$/);

    // Liga B: kreatorov tim se automatski preseli u nju (više nismo u A)
    const leagueB = await createLeague(`E2E Join Liga B ${stamp}`);

    // Pridruživanje ligi A važećom šifrom
    await gotoOk(page, '/League/Join');
    await page.locator('#joinCodeInput').fill(leagueA.code);
    await page.getByRole('button', { name: 'Pridruži se', exact: true }).click();

    await expect(page).toHaveURL(new RegExp(`/liga/${leagueA.id}`));
    await expect(page.locator('#joinInfoToast')).toContainText('Uspješno si se pridružio');
    // Naš tim se sada vidi u poretku lige A
    await expect(page.locator('.ff-table').first()).toContainText(teamName);

    // Ponovno pridruživanje istoj ligi → informativna poruka, bez greške
    await gotoOk(page, '/League/Join');
    await page.locator('#joinCodeInput').fill(leagueA.code);
    await page.getByRole('button', { name: 'Pridruži se', exact: true }).click();
    await expect(page.locator('#joinInfoToast')).toContainText('Već si član');

    // Čišćenje: obriši obje lige (tim ostane bez lige — isto stanje kao
    // nakon postojećeg CRUD testa u leagues.spec.ts)
    for (const league of [leagueB, leagueA]) {
      await gotoOk(page, `/League/Delete/${league.id}`);
      await page.getByRole('button', { name: 'Obriši ligu', exact: true }).click();
      await expect(page).toHaveURL(/\/lige/);
    }
  });
});

// ---- Novi korisnik: registracija → build momčadi → početni sastav ----

test.describe('novi korisnik — puni put', () => {
  test.use(anonymousState);

  test('registracija → auto-popuni momčad → build → spremanje početnog sastava', async ({ page }) => {
    const stamp = Date.now();

    // Registracija (svaki run ostavi novog korisnika u bazi — kao pravi signup)
    await gotoOk(page, '/Account/Register');
    await page.locator('#Email').fill(`e2e.user.${stamp}@example.com`);
    await page.locator('#Password').fill('lozinka123');
    await page.locator('#ConfirmPassword').fill('lozinka123');
    await page.locator('#OIB').fill('12345678901');
    await page.locator('#JMBG').fill('1234567890123');
    await page.getByRole('button', { name: 'Registriraj se', exact: true }).click();

    // Novi korisnik (bez tima) ide ravno na Build
    await expect(page).toHaveURL(/\/FantasyTeam\/Build/i);

    // Sastavi momčad: naziv + auto-popuni 15 igrača unutar svih pravila
    await page.locator('#TeamName').fill(`E2E Tim ${stamp}`);
    await page.locator('#ffAutocompleteBtn').click();
    await expect(page.locator('#ffSelectedInputs input[name="SelectedPlayerIds"]')).toHaveCount(15);

    const completeBtn = page.locator('#ffCompleteBtn');
    await expect(completeBtn).toBeEnabled();
    await completeBtn.click();

    // Uspješan build → preusmjereni smo s Builda, prijavljeni s timom
    await expect(page.getByRole('button', { name: 'Odjava' })).toBeVisible();
    expect(page.url()).not.toContain('/FantasyTeam/Build');

    // My Team novog tima: 11 na terenu + 4 na klupi
    await gotoOk(page, '/FantasyTeam/MyTeam');
    await expect(page.locator('[data-role="pitch"] .ff-pitch-player')).toHaveCount(11);
    await expect(page.locator('[data-role="bench"] .ff-pitch-player')).toHaveCount(4);

    // Odabir početnog sastava: klupaš umjesto startera iste pozicije + save
    const { benchId } = await swapFirstBenchIntoLineup(page);
    await saveLineupAndExpectConfirmation(page);
    await expect(pitchPlayer(page, benchId)).toBeVisible();

    // Transfer: prodaj prvog s klupe (nije u sastavu pa ne resetira lineup) i
    // kupi zamjenu. Tim je jednokratan pa eventualna bodovna kazna ne smeta —
    // ovime je transfer tok pokriven i kad se demo-test gore preskoči.
    const soldTile = page.locator('[data-role="bench"] .ff-pitch-player').first();
    const outId = (await soldTile.getAttribute('data-player-id'))!;
    const posVar = ((await soldTile.getAttribute('data-position')) ?? '').toLowerCase();

    await gotoOk(page, '/transferi');
    await markForSale(page, outId);
    const inId = await buyCheapestAcceptedCandidate(page, posVar);
    await confirmTransfers(page);

    // Kupljeni je u sastavu, prodani na tržištu, momčad i dalje broji 15
    await expect(page.locator(`.ff-transfer__pitch-col .ff-pitch-player[data-player-id="${inId}"]`)).toBeVisible();
    await expect(page.locator(`[data-role="market-card"][data-player-id="${outId}"]`)).toBeEnabled();
    await expect(page.locator('.ff-transfer__pitch-col .ff-pitch-player')).toHaveCount(15);
  });
});
