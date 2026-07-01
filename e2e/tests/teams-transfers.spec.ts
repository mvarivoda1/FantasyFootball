import { test, expect, gotoOk } from './helpers';

// ---- Fantasy Teams ----

test('lista timova prikazuje ranking tablicu', async ({ page }) => {
  await gotoOk(page, '/FantasyTeam');
  await expect(page.locator('h1.ff-page-title')).toContainText('Fantasy Teams');
  await expect(page.locator('#ff-ranking-table')).toBeVisible();
});

test('klik na red tablice vodi na detalje tima', async ({ page }) => {
  await gotoOk(page, '/FantasyTeam');
  const firstRow = page.locator('.ff-ranking-row').first();
  test.skip((await firstRow.count()) === 0, 'Nema timova.');

  await firstRow.click();
  await expect(page).toHaveURL(/\/FantasyTeam\/Details\//);
  await expect(page.locator('.ff-hero__title')).toBeVisible();
});

test('MyTeam prikazuje teren i naziv tima', async ({ page }) => {
  await gotoOk(page, '/FantasyTeam/MyTeam');
  expect(page.url()).not.toContain('/Account/Login');
  await expect(page.locator('.ff-myteam__title')).toBeVisible();
  await expect(page.locator("[data-role='pitch']")).toBeVisible();
});

test('MyTeam: spremanje sastava vraća potvrdu', async ({ page }) => {
  await gotoOk(page, '/FantasyTeam/MyTeam');

  const saveBtn = page.locator("[data-role='save-lineup']");
  await expect(saveBtn).toBeEnabled();
  await saveBtn.click();

  await expect(page).toHaveURL(/\/FantasyTeam\/MyTeam|\/transferi|\/Home/);
  await expect(page.locator('body')).toContainText('uspješno spremljen');
});

test('MyTeam: link Uredi tim otvara Edit formu', async ({ page }) => {
  await gotoOk(page, '/FantasyTeam/MyTeam');
  await page.getByRole('link', { name: 'Uredi tim' }).click();
  await expect(page).toHaveURL(/\/FantasyTeam\/Edit\//);
  await expect(page.locator('#Name')).toBeVisible();
});

// ---- Transferi ----

test('transfer tržište se renderira', async ({ page }) => {
  await gotoOk(page, '/transferi');
  expect(page.url()).not.toContain('/Account/Login');
  await expect(page.locator('.ff-myteam__title')).toBeVisible();
});

test('statistika transfera se renderira', async ({ page }) => {
  await gotoOk(page, '/transferi/statistika');
  expect(page.url()).not.toContain('/Account/Login');
  await expect(page.locator('h1')).toContainText('Transfer Market');
});
