import { test as setup, expect } from '@playwright/test';
import { DEMO_EMAIL, DEMO_PASSWORD } from './helpers';

const authFile = 'playwright/.auth/user.json';

// Jednokratna prijava demo računom — sprema kolačiće u storageState koji
// koriste svi ostali (prijavljeni) testovi.
setup('authenticate', async ({ page }) => {
  await page.goto('/Account/Login', { waitUntil: 'domcontentloaded' });
  await page.locator('#Email').fill(DEMO_EMAIL);
  await page.locator('#Password').fill(DEMO_PASSWORD);
  await page.getByRole('button', { name: 'Prijavi se', exact: true }).click();

  // Uspješna prijava -> u navbaru se pojavi gumb "Odjava".
  await expect(page.getByRole('button', { name: 'Odjava' })).toBeVisible();

  await page.context().storageState({ path: authFile });
});
