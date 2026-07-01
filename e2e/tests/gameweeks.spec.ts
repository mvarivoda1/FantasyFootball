import { test, expect, gotoOk, getFirstId, isAdmin } from './helpers';

// Napomena: kola se NE seedaju automatski (kreira ih admin), pa se testovi koji
// ovise o postojanju kola preskaču ako ih nema.

test('lista kola se renderira', async ({ page }) => {
  await gotoOk(page, '/kola');
  await expect(page.locator('h1.ff-page-title')).toContainText('Gameweeks');
  await expect(page.locator('#ffGwSearch')).toBeVisible();
});

test('pretraga bez pogotka prikazuje prazno stanje', async ({ page }) => {
  await gotoOk(page, '/kola');
  const cards = await page.locator('.ff-gw-card-wrap').count();
  test.skip(cards === 0, 'Nema kreiranih kola.');

  await page.locator('#ffGwSearch').fill('999999');
  await expect(page.locator('#ffGwEmptyResult')).toBeVisible();
});

test('detalji kola se učitavaju', async ({ page, request }) => {
  const id = await getFirstId(request, '/api/gameweek');
  test.skip(id === null, 'Nema kola u bazi.');
  await gotoOk(page, `/kolo/${id}`);
  expect(page.url()).not.toContain('/Account/Login');
  await expect(page.locator('#ffNavbar')).toBeVisible();
});

test('[admin] Gameweek Create forma se učitava', async ({ page }) => {
  test.skip(!(await isAdmin(page)), 'Demo račun nema Admin rolu.');
  await gotoOk(page, '/Gameweek/Create');
  expect(page.url()).not.toContain('/Account/Login');
  await expect(page.locator('main form').first()).toBeVisible();
});
