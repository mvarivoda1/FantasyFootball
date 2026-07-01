import { test, expect, gotoOk } from './helpers';

test('navbar live-dropdown prikazuje prijedloge', async ({ page }) => {
  await gotoOk(page, '/');
  // MIN_CHARS = 2, debounce 250ms pa dohvat /Search/Suggest.
  await page.locator('#ffNavSearchInput').fill('ha');
  await expect(page.locator('#ffNavSearchResults .ff-navsearch__item').first()).toBeVisible();
});

test('navbar pretraga na Enter vodi na stranicu rezultata', async ({ page }) => {
  await gotoOk(page, '/');
  await page.locator('#ffNavSearchInput').fill('Haaland');
  await page.locator('#ffNavSearchInput').press('Enter');

  await expect(page).toHaveURL(/\/Search\?q=Haaland/);
  await expect(page.locator('body')).toContainText('Haaland');
});

test('stranica rezultata prikazuje kartice za poznati pojam', async ({ page }) => {
  await gotoOk(page, '/Search?q=Haaland');
  await expect(page.locator('h1.ff-page-title')).toContainText('Pretraga');
  await expect(page.locator('.ff-result-card').first()).toBeVisible();
});

test('nepostojeći pojam prikazuje prazno stanje', async ({ page }) => {
  await gotoOk(page, '/Search?q=zzzznemarezultatazzz');
  await expect(page.locator('.ff-empty')).toBeVisible();
  await expect(page.locator('body')).toContainText('Nismo pronašli');
});
