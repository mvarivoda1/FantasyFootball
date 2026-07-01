import { test, expect, gotoOk } from './helpers';

test('lista liga prikazuje tablicu', async ({ page }) => {
  await gotoOk(page, '/lige');
  await expect(page.locator('h1.ff-page-title')).toContainText('Leagues');
  await expect(page.locator('#ff-leagues-table')).toBeVisible();
});

test('sortiranje po zaglavlju radi', async ({ page }) => {
  await gotoOk(page, '/lige');
  const nameHeader = page.locator("th[data-sort-key='name']");
  await nameHeader.click();
  await expect(nameHeader).toHaveAttribute('aria-sort', /ascending|descending/);
});

test('pridruživanje ligi s neispravnom šifrom prikazuje grešku', async ({ page }) => {
  await gotoOk(page, '/League/Join');
  await page.locator('#joinCodeInput').fill('ZZZZZZ');
  await page.getByRole('button', { name: 'Pridruži se', exact: true }).click();
  await expect(page.locator('body')).toContainText('ne postoji');
});

/**
 * Puni CRUD tok kroz UI (kreiraj → uredi → obriši). Kreiranje lige premješta
 * demo tim u novu ligu; brisanje na kraju odveže tim, pa nema zaostale lige.
 */
test('liga: kreiraj → uredi → obriši (UI)', async ({ page }) => {
  const unique = `E2E Liga ${Date.now()}`;

  // Create
  await gotoOk(page, '/League/Create');
  await page.locator('#Name').fill(unique);
  await page.locator('#MaxTeams').fill('10');
  await page.getByRole('button', { name: 'Kreiraj ligu', exact: true }).click();

  await expect(page).toHaveURL(/\/League\/Created\/\d+/);
  await expect(page.locator('body')).toContainText(unique);

  const m = page.url().match(/\/(\d+)(?:[/?#]|$)/);
  expect(m).not.toBeNull();
  const leagueId = Number(m![1]);

  // Edit (Opis je obavezno polje!)
  const edited = `${unique} (edit)`;
  await gotoOk(page, `/League/Edit/${leagueId}`);
  await expect(page.locator('#Name')).toHaveValue(unique);
  await page.locator('#Name').fill(edited);
  await page.locator('#Description').fill('E2E opis lige');
  await page.getByRole('button', { name: 'Spremi promjene', exact: true }).click();

  await expect(page).toHaveURL(new RegExp(`/liga/${leagueId}`));
  await expect(page.locator('body')).toContainText(edited);

  // Delete
  await gotoOk(page, `/League/Delete/${leagueId}`);
  await page.getByRole('button', { name: 'Obriši ligu', exact: true }).click();

  await expect(page).toHaveURL(/\/lige/);
  await expect(page.locator('#ffLeaguesTbody')).not.toContainText(edited);
});
