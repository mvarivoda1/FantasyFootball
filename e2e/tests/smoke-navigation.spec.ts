import { test, expect, gotoOk, getFirstId, getMyTeamId } from './helpers';

/**
 * Široki "ništa se ne ruši" smoke test: prijavljeni korisnik posjeti svaku
 * glavnu i detaljnu stranicu. Tvrdimo: status < 400, nismo na loginu, navbar je
 * vidljiv, i (preko pageGuard fixture-a) nema neuhvaćene JS iznimke.
 */
test.describe('Smoke — sve glavne stranice se učitavaju', () => {
  const primary: [string, string][] = [
    ['/', 'Dashboard (Home)'],
    ['/igraci', 'Players list'],
    ['/lige', 'Leagues list'],
    ['/kola', 'Gameweeks list'],
    ['/transferi', 'Transfer market'],
    ['/transferi/statistika', 'Transfer stats'],
    ['/FantasyTeam', 'Fantasy teams list'],
    ['/FantasyTeam/MyTeam', 'My Team'],
    ['/League/Create', 'Create league form'],
    ['/League/Join', 'Join league form'],
    ['/Search?q=a', 'Global search results'],
  ];

  for (const [route, label] of primary) {
    test(`učitava se: ${label}`, async ({ page }) => {
      await gotoOk(page, route);
      expect(page.url(), 'preusmjereni na login').not.toContain('/Account/Login');
      await expect(page.locator('#ffNavbar')).toBeVisible();
    });
  }

  test('učitava se: Edit my team form', async ({ page }) => {
    const teamId = await getMyTeamId(page);
    await gotoOk(page, `/FantasyTeam/Edit/${teamId}`);
    expect(page.url()).not.toContain('/Account/Login');
    await expect(page.locator('#Name')).toBeVisible();
  });
});

test.describe('Smoke — detaljne stranice (ID iz REST-a)', () => {
  const details: [string, string, string][] = [
    ['/api/player', '/igrac', 'player details'],
    ['/api/league', '/liga', 'league details'],
    ['/api/gameweek', '/kolo', 'gameweek details'],
    ['/api/fantasyteam', '/FantasyTeam/Details', 'team details'],
    ['/api/transfer', '/Transfer/Details', 'transfer details'],
  ];

  for (const [apiPath, urlPrefix, label] of details) {
    test(`učitava se: ${label}`, async ({ page, request }) => {
      const id = await getFirstId(request, apiPath);
      test.skip(id === null, `Nema zapisa za ${apiPath}.`);
      await gotoOk(page, `${urlPrefix}/${id}`);
      expect(page.url()).not.toContain('/Account/Login');
      await expect(page.locator('#ffNavbar')).toBeVisible();
    });
  }
});

test('Smoke — klik kroz sve navbar linkove ne ruši aplikaciju', async ({ page }) => {
  const targets: [string, RegExp][] = [
    ['/igraci', /\/igraci/],
    ['/FantasyTeam', /\/FantasyTeam/],
    ['/lige', /\/lige/],
    ['/kola', /\/kola/],
    ['/transferi', /\/transferi/],
    ['/FantasyTeam/MyTeam', /\/FantasyTeam\/MyTeam/],
  ];

  await gotoOk(page, '/');
  for (const [href, expectUrl] of targets) {
    await page.locator(`#ffNavbar a[href='${href}']`).first().click();
    await page.waitForURL(expectUrl);
    await expect(page.locator('#ffNavbar')).toBeVisible();
    expect(page.url()).not.toContain('/Account/Login');
  }
});
