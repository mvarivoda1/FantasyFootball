import { defineConfig, devices } from '@playwright/test';

/**
 * FantasyFootball E2E — Playwright konfiguracija.
 *
 * PREDUVJET: aplikacija je pokrenuta (default http://localhost:5300 — namjerno
 * različit od dev porta 5263 da se test-instanca i dev-instanca ne sudaraju) i baza je
 * seedana (demo račun marko@gmail.com / markopass — prvi seedani korisnik, ima
 * fantasy tim i Admin rolu). Bazni URL se mijenja env varijablom FF_BASE_URL.
 *
 * Autentikacija se izvrši jednom u `auth.setup.ts` i sprema u storageState koji
 * koriste svi prijavljeni testovi. Anonimni testovi (login/registracija/zaštita
 * ruta) sami resetiraju storageState na prazno.
 */
const baseURL = process.env.FF_BASE_URL ?? 'http://localhost:5300';
const authFile = 'playwright/.auth/user.json';

export default defineConfig({
  testDir: './tests',
  fullyParallel: true,
  forbidOnly: !!process.env.CI,
  retries: process.env.CI ? 1 : 0,
  workers: process.env.CI ? 1 : undefined,

  // Vizualni HTML izvještaj (+ sažetak u konzoli).
  reporter: [
    ['html', { open: 'never' }],
    ['list'],
  ],

  use: {
    baseURL,
    ignoreHTTPSErrors: true,
    locale: 'hr-HR',
    trace: 'on-first-retry',
    screenshot: 'only-on-failure',
    video: 'retain-on-failure',
  },

  projects: [
    // 1) Prijava — sprema storageState za ostale projekte.
    { name: 'setup', testMatch: /auth\.setup\.ts/ },

    // 2) Glavni testovi — prijavljeni preko spremljenog storageState-a.
    {
      name: 'chromium',
      use: { ...devices['Desktop Chrome'], storageState: authFile },
      dependencies: ['setup'],
    },
  ],
});
