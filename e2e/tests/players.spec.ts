import { test, expect, gotoOk, getFirstId, isAdmin } from './helpers';

test('lista igrača prikazuje locker kartice', async ({ page }) => {
  await gotoOk(page, '/igraci');
  await expect(page.locator('h1.ff-page-title')).toContainText('Players');
  await expect(page.locator('.ff-locker').first()).toBeVisible();
});

test('live-search filtrira igrače (AJAX /Player/Search)', async ({ page }) => {
  await gotoOk(page, '/igraci');
  const total = await page.locator('.ff-locker').count();
  test.skip(total <= 1, 'Premalo igrača za smislen test.');

  await page.locator('#filterSearch').fill('Haaland');
  await expect(page.locator('#filterCount')).toContainText('Showing 1 of');
  await expect(page.locator('.ff-locker:visible')).toHaveCount(1);
});

test('brisanje pretrage vraća sve kartice', async ({ page }) => {
  await gotoOk(page, '/igraci');
  const total = await page.locator('.ff-locker').count();

  await page.locator('#filterSearch').fill('Haaland');
  await expect(page.locator('#filterCount')).toContainText('Showing 1 of');

  await page.locator('#filterSearchClear').click();
  await expect(page.locator('.ff-locker:visible')).toHaveCount(total);
});

test('filter po poziciji smanjuje broj kartica', async ({ page }) => {
  await gotoOk(page, '/igraci');
  const total = await page.locator('.ff-locker').count();

  await page.locator('#filterPosition').selectOption('Goalkeeper');
  await expect(page.locator('.ff-locker:visible').first()).toBeVisible();
  expect(await page.locator('.ff-locker:visible').count()).toBeLessThan(total);
});

test('klik na locker otvara i zatvara modal', async ({ page }) => {
  await gotoOk(page, '/igraci');
  await page.locator('.ff-locker').first().click();

  const backdrop = page.locator('#lockerModalBackdrop');
  await expect(backdrop).toHaveClass(/ff-locker-modal--visible/);
  await expect(page.locator('#lockerModalName')).not.toBeEmpty();

  await page.locator('#lockerModalClose').click();
  await expect(backdrop).not.toHaveClass(/ff-locker-modal--visible/);
});

test('detalji igrača se renderiraju', async ({ page, request }) => {
  const id = await getFirstId(request, '/api/player');
  test.skip(id === null, 'Nema igrača u bazi.');
  await gotoOk(page, `/igrac/${id}`);
  await expect(page.locator('.ff-hero__title')).toBeVisible();
});

test('nepostojeći igrač vraća 404', async ({ page }) => {
  const resp = await page.goto('/igrac/99999999', { waitUntil: 'domcontentloaded' });
  expect(resp?.status()).toBe(404);
});

test('[admin] Player Create forma se učitava', async ({ page }) => {
  test.skip(!(await isAdmin(page)), 'Demo račun nema Admin rolu.');
  await gotoOk(page, '/Player/Create');
  expect(page.url()).not.toContain('/Account/Login');
  await expect(page.locator('main form').first()).toBeVisible();
});

test('[admin] Player Edit i Delete forme se učitavaju', async ({ page, request }) => {
  test.skip(!(await isAdmin(page)), 'Demo račun nema Admin rolu.');
  const id = await getFirstId(request, '/api/player');
  test.skip(id === null, 'Nema igrača u bazi.');

  await gotoOk(page, `/Player/Edit/${id}`);
  await expect(page.locator('main form').first()).toBeVisible();

  await gotoOk(page, `/Player/Delete/${id}`);
  await expect(page.locator('#ffNavbar')).toBeVisible();
});
