import { test, expect, gotoOk, anonymousState, DEMO_EMAIL, DEMO_PASSWORD } from './helpers';

// Autentikacija/autorizacija se testira iz ODJAVLJENOG stanja.
test.use(anonymousState);

test('login stranica se renderira', async ({ page }) => {
  await gotoOk(page, '/Account/Login');
  await expect(page.locator('#Email')).toBeVisible();
  await expect(page.locator('#Password')).toBeVisible();
  await expect(page.getByRole('button', { name: 'Prijavi se', exact: true })).toBeVisible();
});

test('ispravna prijava vodi na dashboard i prikazuje Odjava', async ({ page }) => {
  await gotoOk(page, '/Account/Login');
  await page.locator('#Email').fill(DEMO_EMAIL);
  await page.locator('#Password').fill(DEMO_PASSWORD);
  await page.getByRole('button', { name: 'Prijavi se', exact: true }).click();

  await expect(page.getByRole('button', { name: 'Odjava' })).toBeVisible();
  await expect(page.locator('#ffNavbar')).toContainText(DEMO_EMAIL);
});

test('neispravna prijava prikazuje grešku i ostaje na loginu', async ({ page }) => {
  await gotoOk(page, '/Account/Login');
  await page.locator('#Email').fill(DEMO_EMAIL);
  await page.locator('#Password').fill('pogresna-lozinka');
  await page.getByRole('button', { name: 'Prijavi se', exact: true }).click();

  await expect(page.locator('#Email')).toBeVisible();
  await expect(page.locator('body')).toContainText('Neispravan email ili lozinka');
  await expect(page.getByRole('button', { name: 'Odjava' })).toHaveCount(0);
});

test('registracija se renderira sa svim poljima', async ({ page }) => {
  await gotoOk(page, '/Account/Register');
  for (const id of ['#Email', '#Password', '#ConfirmPassword', '#OIB', '#JMBG']) {
    await expect(page.locator(id)).toBeVisible();
  }
  await expect(page.getByRole('button', { name: 'Registriraj se', exact: true })).toBeVisible();
});

test('registracija s neusklađenim lozinkama pokreće client-side validaciju', async ({ page }) => {
  await gotoOk(page, '/Account/Register');
  await page.locator('#Email').fill('novikorisnik@example.com');
  await page.locator('#Password').fill('lozinka123');
  await page.locator('#ConfirmPassword').fill('nesto-drugo');
  await page.locator('#OIB').fill('12345678901');
  await page.locator('#JMBG').fill('1234567890123');
  await page.getByRole('button', { name: 'Registriraj se', exact: true }).click();

  // jQuery validacija sprječava submit -> ostajemo na formi, nismo prijavljeni.
  await expect(page.locator('#ConfirmPassword')).toBeVisible();
  await expect(page.getByRole('button', { name: 'Odjava' })).toHaveCount(0);
});

test('odjava vraća u anonimno stanje', async ({ page }) => {
  await gotoOk(page, '/Account/Login');
  await page.locator('#Email').fill(DEMO_EMAIL);
  await page.locator('#Password').fill(DEMO_PASSWORD);
  await page.getByRole('button', { name: 'Prijavi se', exact: true }).click();
  await expect(page.getByRole('button', { name: 'Odjava' })).toBeVisible();

  await page.getByRole('button', { name: 'Odjava' }).click();
  await expect(page.getByRole('link', { name: 'Prijava' })).toBeVisible();
});

test('zaštićena stranica preusmjerava anonimnog korisnika na login', async ({ page }) => {
  await gotoOk(page, '/kola');
  await expect(page).toHaveURL(/\/Account\/Login/);
  await expect(page.locator('#Email')).toBeVisible();
});

test('AccessDenied stranica se renderira', async ({ page }) => {
  await gotoOk(page, '/Account/AccessDenied');
  await expect(page.locator('#ffNavbar')).toBeVisible();
});

test('Search je dostupan anonimno', async ({ page }) => {
  await gotoOk(page, '/Search?q=a');
  expect(page.url()).not.toContain('/Account/Login');
  await expect(page.locator('h1.ff-page-title')).toContainText('Pretraga');
});
