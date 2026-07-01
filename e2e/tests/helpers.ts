import { test as base, expect, Page, APIRequestContext } from '@playwright/test';

export const DEMO_EMAIL = 'marko@gmail.com';
export const DEMO_PASSWORD = 'markopass';

/**
 * Prošireni `test` — automatski pada ako se na stranici dogodi neuhvaćena JS
 * iznimka (`pageerror`). console.error se samo bilježi (npr. 404 za CDN resurs),
 * ali ne ruši test. Auto-fixture pa se ne mora eksplicitno tražiti u testu.
 */
export const test = base.extend<{ pageGuard: void }>({
  pageGuard: [
    async ({ page }, use, testInfo) => {
      const errors: string[] = [];
      page.on('pageerror', (e) => errors.push(e.message));
      await use();
      if (testInfo.status === testInfo.expectedStatus && errors.length > 0) {
        throw new Error(`Neuhvaćena JS iznimka na stranici:\n${errors.join('\n')}`);
      }
    },
    { auto: true },
  ],
});

export { expect };

/** Navigira i tvrdi da odgovor nije server/klijent greška (status < 400). */
export async function gotoOk(page: Page, path: string) {
  const resp = await page.goto(path, { waitUntil: 'domcontentloaded' });
  expect(resp, `Nema HTTP odgovora za ${path}`).not.toBeNull();
  expect(resp!.status(), `HTTP ${resp!.status()} za ${path}`).toBeLessThan(400);
  return resp!;
}

/** Dohvati prvi `id` iz REST liste (npr. /api/player) ili null ako je prazna. */
export async function getFirstId(request: APIRequestContext, apiPath: string): Promise<number | null> {
  const resp = await request.get(apiPath);
  if (!resp.ok()) return null;
  const data = await resp.json();
  if (!Array.isArray(data) || data.length === 0) return null;
  return typeof data[0]?.id === 'number' ? data[0].id : null;
}

/** ID fantasy tima prijavljenog korisnika (iz skrivenog polja na MyTeam stranici). */
export async function getMyTeamId(page: Page): Promise<number> {
  await gotoOk(page, '/FantasyTeam/MyTeam');
  const val = await page.locator('#ffMyTeamForm input[name="teamId"]').getAttribute('value');
  return Number(val);
}

/** Je li prijavljeni korisnik Admin? (provjera preko admin-only akcije na listi igrača) */
export async function isAdmin(page: Page): Promise<boolean> {
  await gotoOk(page, '/igraci');
  return (await page.locator('text=Dodaj igrača').count()) > 0;
}

/** Prazan storageState — koristi ga u anonimnim testovima (odjavljeni korisnik). */
export const anonymousState = { storageState: { cookies: [], origins: [] } };
