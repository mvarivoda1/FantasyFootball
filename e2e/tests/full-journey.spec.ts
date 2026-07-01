import { test, expect, anonymousState, DEMO_EMAIL, DEMO_PASSWORD } from './helpers';

// Port originalnog 10-koračnog scenarija (FullJourneyTests). Kreće odjavljen.
test.use(anonymousState);

test('puni korisnički put — 10 koraka', async ({ page }) => {
  // 1. Stranica za prijavu
  await page.goto('/Account/Login', { waitUntil: 'domcontentloaded' });
  await expect(page.locator('#Email')).toBeVisible();

  // 2. Prijava demo računom
  await page.locator('#Email').fill(DEMO_EMAIL);
  await page.locator('#Password').fill(DEMO_PASSWORD);
  await page.getByRole('button', { name: 'Prijavi se', exact: true }).click();

  // 3. Potvrda prijave
  await expect(page.getByRole('button', { name: 'Odjava' })).toBeVisible();

  // 4. Globalna pretraga preko navbara
  await page.locator('#ffNavSearchInput').fill('a');
  await page.locator('#ffNavSearchInput').press('Enter');
  await expect(page).toHaveURL(/\/Search/);

  // 5. Lista igrača
  await page.goto('/igraci', { waitUntil: 'domcontentloaded' });
  await expect(page.locator('body')).toContainText('Haaland');

  // 6. Detalji igrača
  await page.goto('/igrac/1', { waitUntil: 'domcontentloaded' });
  await expect(page).toHaveURL(/\/igrac\/1/);

  // 7. Lista liga
  await page.goto('/lige', { waitUntil: 'domcontentloaded' });
  await expect(page.locator('body')).toContainText('Liga');

  // 8. Lista kola
  await page.goto('/kola', { waitUntil: 'domcontentloaded' });
  await expect(page.locator('body')).toContainText('Gameweek');

  // 9. Lista transfera
  await page.goto('/transferi', { waitUntil: 'domcontentloaded' });
  await expect(page.locator('body')).toContainText('Transfer');

  // 10. Odjava
  await page.getByRole('button', { name: 'Odjava' }).click();
  await expect(page.getByRole('link', { name: 'Prijava' })).toBeVisible();
});
