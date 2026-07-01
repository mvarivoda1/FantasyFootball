import { test, expect, APIRequestContext } from '@playwright/test';

/**
 * REST API — potpuna pokrivenost svih 5 kontrolera × 5 endpointa = 25 ruta:
 *   GET list, GET {id}, POST, PUT, DELETE.
 *
 * Koristi ugrađeni `request` fixture koji nasljeđuje storageState (prijava kao
 * marko — Admin), pa prolaze i write-endpointi zaštićeni [Authorize]/Admin.
 *
 * Write testovi su samodostatni CRUD krugovi (POST → GET → PUT → DELETE) pa
 * ne ostavljaju smeće u bazi.
 *
 * Enumi (default System.Text.Json) šalju se numerički:
 *   Position:  Goalkeeper=0, Defender=1, Midfielder=2, Forward=3
 *   Direction: In=0, Out=1
 */

const controllers = ['player', 'league', 'gameweek', 'fantasyteam', 'transfer'] as const;

async function getFirstId(request: APIRequestContext, path: string): Promise<number | null> {
  const resp = await request.get(path);
  if (!resp.ok()) return null;
  const data = await resp.json();
  return Array.isArray(data) && data.length > 0 && typeof data[0]?.id === 'number' ? data[0].id : null;
}

// ---------------------------------------------------------------------------
// GET (read) — liste, ?q filter, i 404 za nepostojeći id — za svih 5 kontrolera
// ---------------------------------------------------------------------------
test.describe('API — read endpoints', () => {
  for (const c of controllers) {
    test(`GET /api/${c} vraća JSON polje`, async ({ request }) => {
      const resp = await request.get(`/api/${c}`);
      expect(resp.status(), await resp.text().catch(() => '')).toBe(200);
      const data = await resp.json();
      expect(Array.isArray(data)).toBeTruthy();
    });

    test(`GET /api/${c}?q=a vraća JSON polje`, async ({ request }) => {
      const resp = await request.get(`/api/${c}?q=a`);
      expect(resp.status()).toBe(200);
      expect(Array.isArray(await resp.json())).toBeTruthy();
    });

    test(`GET /api/${c}/{id} vraća objekt (ili preskoči ako je baza prazna)`, async ({ request }) => {
      const id = await getFirstId(request, `/api/${c}`);
      test.skip(id === null, `Nema zapisa za /api/${c}.`);
      const resp = await request.get(`/api/${c}/${id}`);
      expect(resp.status()).toBe(200);
      const obj = await resp.json();
      expect(obj).toMatchObject({ id });
    });

    test(`GET /api/${c}/99999999 vraća 404`, async ({ request }) => {
      const resp = await request.get(`/api/${c}/99999999`);
      expect(resp.status()).toBe(404);
    });
  }
});

// ---------------------------------------------------------------------------
// PLAYER — CRUD (POST/PUT/DELETE zahtijevaju Admin rolu; marko ju ima)
// ---------------------------------------------------------------------------
test('API — player CRUD (POST → GET → PUT → DELETE)', async ({ request }) => {
  const create = await request.post('/api/player', {
    data: {
      firstName: 'E2E',
      lastName: 'ApiTester',
      position: 2, // Midfielder
      club: 'E2E FC',
      nationality: 'Croatia',
      dateOfBirth: '1995-05-05T00:00:00Z',
      marketValue: 5.0,
      goals: 0,
      assists: 0,
      cleanSheets: 0,
      totalPoints: 0,
    },
  });
  expect(create.status(), await create.text().catch(() => '')).toBe(201);
  const created = await create.json();
  const id: number = created.id;
  expect(id).toBeGreaterThan(0);

  const read = await request.get(`/api/player/${id}`);
  expect(read.status()).toBe(200);
  expect((await read.json()).lastName).toBe('ApiTester');

  const update = await request.put(`/api/player/${id}`, {
    data: {
      id,
      firstName: 'E2E',
      lastName: 'ApiUpdated',
      position: 3, // Forward
      club: 'E2E FC',
      nationality: 'Croatia',
      dateOfBirth: '1995-05-05T00:00:00Z',
      marketValue: 6.5,
      goals: 1,
      assists: 2,
      cleanSheets: 0,
      totalPoints: 10,
    },
  });
  expect(update.status(), await update.text().catch(() => '')).toBe(200);
  expect((await update.json()).lastName).toBe('ApiUpdated');

  const del = await request.delete(`/api/player/${id}`);
  expect(del.status()).toBe(204);

  expect((await request.get(`/api/player/${id}`)).status()).toBe(404);
});

// ---------------------------------------------------------------------------
// LEAGUE — CRUD (POST/PUT/DELETE: [Authorize]) — bez nuspojava na demo tim
// ---------------------------------------------------------------------------
test('API — league CRUD (POST → GET → PUT → DELETE)', async ({ request }) => {
  const create = await request.post('/api/league', {
    data: { name: 'E2E API Liga', season: '2026/2027', maxTeams: 10, description: 'test' },
  });
  expect(create.status(), await create.text().catch(() => '')).toBe(201);
  const id: number = (await create.json()).id;
  expect(id).toBeGreaterThan(0);

  expect((await request.get(`/api/league/${id}`)).status()).toBe(200);

  const update = await request.put(`/api/league/${id}`, {
    data: { name: 'E2E API Liga (edit)', season: '2026/2027', maxTeams: 12, description: 'edited' },
  });
  expect(update.status(), await update.text().catch(() => '')).toBe(200);
  expect((await update.json()).name).toBe('E2E API Liga (edit)');

  expect((await request.delete(`/api/league/${id}`)).status()).toBe(204);
  expect((await request.get(`/api/league/${id}`)).status()).toBe(404);
});

// ---------------------------------------------------------------------------
// FANTASY TEAM — CRUD ([Authorize]) — kreira tim bez igrača, pa ga briše
// ---------------------------------------------------------------------------
test('API — fantasyteam CRUD (POST → GET → PUT → DELETE)', async ({ request }) => {
  const create = await request.post('/api/fantasyteam', {
    data: { name: 'E2E API Tim', ownerName: 'E2E Owner' },
  });
  expect(create.status(), await create.text().catch(() => '')).toBe(201);
  const id: number = (await create.json()).id;
  expect(id).toBeGreaterThan(0);

  expect((await request.get(`/api/fantasyteam/${id}`)).status()).toBe(200);

  const update = await request.put(`/api/fantasyteam/${id}`, {
    data: { name: 'E2E API Tim (edit)', ownerName: 'E2E Owner' },
  });
  expect(update.status(), await update.text().catch(() => '')).toBe(200);
  expect((await update.json()).name).toBe('E2E API Tim (edit)');

  expect((await request.delete(`/api/fantasyteam/${id}`)).status()).toBe(204);
  expect((await request.get(`/api/fantasyteam/${id}`)).status()).toBe(404);
});

// ---------------------------------------------------------------------------
// GAMEWEEK — CRUD (Admin) — traži slobodan broj kola (1..38, mora biti jedinstven)
// ---------------------------------------------------------------------------
test('API — gameweek CRUD (POST → GET → PUT → DELETE)', async ({ request }) => {
  const existing = await (await request.get('/api/gameweek')).json();
  const used = new Set<number>((existing as Array<{ weekNumber: number }>).map((g) => g.weekNumber));
  let week = 0;
  for (let n = 1; n <= 38; n++) {
    if (!used.has(n)) {
      week = n;
      break;
    }
  }
  test.skip(week === 0, 'Sva kola (1–38) su zauzeta.');

  const create = await request.post('/api/gameweek', {
    data: { weekNumber: week, startDate: '2031-01-01T12:00:00Z', endDate: '2031-01-08T12:00:00Z' },
  });
  expect(create.status(), await create.text().catch(() => '')).toBe(201);
  const id: number = (await create.json()).id;
  expect(id).toBeGreaterThan(0);

  expect((await request.get(`/api/gameweek/${id}`)).status()).toBe(200);

  const update = await request.put(`/api/gameweek/${id}`, {
    data: { id, weekNumber: week, startDate: '2031-01-02T12:00:00Z', endDate: '2031-01-09T12:00:00Z' },
  });
  expect(update.status(), await update.text().catch(() => '')).toBe(200);

  expect((await request.delete(`/api/gameweek/${id}`)).status()).toBe(204);
  expect((await request.get(`/api/gameweek/${id}`)).status()).toBe(404);
});

// ---------------------------------------------------------------------------
// TRANSFER — CRUD ([Authorize]) — referencira postojećeg igrača i tim
// ---------------------------------------------------------------------------
test('API — transfer CRUD (POST → GET → PUT → DELETE)', async ({ request }) => {
  const playerId = await getFirstId(request, '/api/player');
  const teamId = await getFirstId(request, '/api/fantasyteam');
  test.skip(playerId === null || teamId === null, 'Nedostaje igrač ili tim u bazi.');

  const create = await request.post('/api/transfer', {
    data: { playerId, teamId, direction: 0 /* In */, price: 5.0 },
  });
  expect(create.status(), await create.text().catch(() => '')).toBe(201);
  const id: number = (await create.json()).id;
  expect(id).toBeGreaterThan(0);

  expect((await request.get(`/api/transfer/${id}`)).status()).toBe(200);

  const update = await request.put(`/api/transfer/${id}`, {
    data: { playerId, teamId, direction: 1 /* Out */, price: 6.5 },
  });
  expect(update.status(), await update.text().catch(() => '')).toBe(200);
  expect((await update.json()).direction).toBe('Out');

  expect((await request.delete(`/api/transfer/${id}`)).status()).toBe(204);
  expect((await request.get(`/api/transfer/${id}`)).status()).toBe(404);
});
