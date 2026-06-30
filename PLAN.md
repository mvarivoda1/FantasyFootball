Ready for review
Select text to add comments on the plan
Plan: In-card gameweek simulation + fixtures in statistics
Context
Today the Gameweeks tab shows 6 hardcoded gameweeks built in Repositories/GameweekMockRepository.cs and seeded by DAL/DbSeeder.cs. Every MatchPerformance.PointsEarned is baked in by hand; player/team points are static. There is no way to "play" a season, and gameweeks have no fixtures.

The user wants to keep the Gameweeks tab UI exactly as it is (cards list, admin "Kreiraj kolo" Create button, Edit/Delete) and instead add a simulation workflow inside an individual gameweek:

Admin creates an (empty) gameweek with the existing Create flow.
Admin opens that gameweek's card → Details page and clicks Simulate.
Simulation randomly pairs the 20 clubs into 10 fixtures, rolls random scorelines, attributes goals/assists/cards/etc. to individual players, and computes fantasy points per FPL-Scoring-Rules.md. Results are shown as a preview (nothing saved yet; admin can re-roll).
Admin clicks Confirm → the fixtures + player performances are persisted, player/team points are updated, and everyone can now see the gameweek's statistics — including the 10 fixture results added to the stats.
Each user gets a slider / arrows in My Team to view their team's score (and per-player points) for any past gameweek.
Decisions confirmed with the user:

Wipe & start fresh — delete the 6 seeded gameweeks and reset all Player.TotalPoints/Goals/Assists/CleanSheets and FantasyTeam.TotalPoints to 0. Points build up only from confirmed simulations. 1b. Player prices come from player-prices.md (real FPL costs, £3.7–£14.7), replacing the old points-derived [£4.5,£13.0] mapping (which had to go anyway once points reset to 0). See §2.
Preview, then confirm — Simulate rolls + displays results without saving (re-rollable); Confirm commits performances + points.
Skip captaincy — fantasy GW score = sum of the 11 starters' points.
Undo allowed — before confirm: re-roll freely; after confirm: admin can Delete the gameweek, which reverses its points (existing Delete button, upgraded).
UI rule (CLAUDE.md): every .cshtml, CSS, and JS change below MUST be implemented by the ux-designer subagent (Dark Stadium theme, custom CSS in wwwroot/css/site.css, no default Bootstrap look). This plan describes what the UI needs; the ux-designer builds it. The existing Gameweeks tab layout stays — no sub-tabs, no restructuring.

1. Data model changes
New entity Models/Fixture.cs
One row per simulated match (10 per gameweek) — shown in the gameweek statistics.

Id, GameweekId (FK), HomeClub (string), AwayClub (string),
HomeGoals (int), AwayGoals (int)
New entity Models/GameweekTeamScore.cs
Snapshot of each fantasy team's result for a gameweek — the authoritative "score you banked", used by the My Team slider and by delete-reversal.

Id, FantasyTeamId (FK), GameweekId (FK), Points (int),
LineupIds (string?, snapshot of the 11 starter IDs at confirm time)
Extend Models/MatchPerformance.cs
Add fields for full FPL scoring + richer stats: int Saves; int GoalsConceded; int Bonus; (CleanSheet, Goals, Assists, YellowCards, RedCards, MinutesPlayed, PointsEarned already exist.)

Models/Gameweek.cs
Add nav ICollection<Fixture> Fixtures (init in ctor). A gameweek is considered simulated iff Fixtures.Any() — no extra status column needed (preview is never persisted, so a created-but-unconfirmed gameweek simply has no fixtures).

DAL/FantasyFootballDbContext.cs
Add DbSet<Fixture> Fixtures, DbSet<GameweekTeamScore> GameweekTeamScores.
Gameweek 1–N Fixture: Cascade (delete-gameweek removes fixtures).
GameweekTeamScore: FK→Gameweek = Cascade, FK→FantasyTeam = Restrict — mirrors the existing MatchPerformance convention (Gameweek=Cascade, Player=Restrict) and avoids SQL Server multiple-cascade-path errors.
EF migration (use the entity-framework skill)
One migration doing schema + a one-time data reset (the reset must live here, because DbSeeder only full-seeds an empty DB — an already-seeded database is reset via migrationBuilder.Sql(...)):

Create Fixtures, GameweekTeamScores; add Saves/GoalsConceded/Bonus.
DELETE FROM MatchPerformances; DELETE FROM Gameweeks;
UPDATE Players SET TotalPoints=0, Goals=0, Assists=0, CleanSheets=0;
UPDATE FantasyTeams SET TotalPoints=0;
Leave Players.MarketValue to the startup price refresh (§2): on the next boot it copies the new player-prices.md costs from the mock onto every player, so no per-player prices need to be encoded in SQL.
Runs once via ctx.Database.Migrate() in Program.cs (no-op on a fresh DB).

2. Player prices (from player-prices.md) + seeding — Repositories/PlayerMockRepository.cs, DAL/DbSeeder.cs
New pricing source. Today every player's MarketValue is overwritten by ApplyPointBasedPrices (linear map TotalPoints → [£4.5,£13.0]). Since points now reset to 0, that map is dropped and replaced with the real FPL costs in player-prices.md (£3.7–£14.7), which already fit the £100 / 15-player budget.

Bake prices into the seed data: set each of the 300 players' MarketValue in PlayerMockRepository.cs to its cost from player-prices.md. Match each mock player to a file row by club (file→DB map: Man City→Manchester City, Man Utd→Manchester United, Spurs→Tottenham, Nott'm Forest→Nottingham Forest, Wolves→Wolverhampton, Leeds→Leeds United; the other 14 identical) + name (normalize diacritics/punctuation; the file uses surname, "Initial.Surname", or first name — e.g. M.Salah→Salah, Virgil→van Dijk, Rúben→Dias, Ederson M.→Ederson Moraes). Use a throwaway script in scratchpad to do the match and report misses; assign unmatched fringe/transferred players (De Bruyne, Ortega, academy fillers) a sensible default (≈£4.5, position-aware). Doing the fuzzy match once at authoring time keeps runtime matching exact and robust.
Drop the points-based remap: remove the ApplyPointBasedPrices calls in the full-seed path (line 66) and in RebalancePrices (line 152). Prices now come straight from the mock.
Refresh existing DBs from the mock: in RebalancePrices, copy MarketValue from the mock onto existing DB players by exact (FirstName,LastName,Club) key (the DB was seeded from the mock ⇒ exact match — reuse the keying already in EnsureAllMockPlayers). Already-seeded databases thus pick up the new FPL prices on next startup without SQL price data. Keep the squad-fill / EnforceConstraints / budget logic so any squad pushed over £100 by the new prices is re-fitted; lower the squad-builder MinPrice heuristic (4.5 → ~4.0) to match the new floor.
Reset (from the wipe decision):

Stop seeding gameweeks: remove the GameweekMockRepository usage (gameweekRepo, gameweeks list, id-reset at lines 56–57, context.Gameweeks.AddRange(gameweeks) at line 83). Leave GameweekMockRepository.cs on disk so GameweekApiController/tests compile.
Zero live counters in the full-seed path: TotalPoints=Goals=Assists=CleanSheets=0 per player and t.TotalPoints = 0 per team (the §1 migration does the same for already-seeded DBs).

3. Simulation engine — Services/GameweekSimulationService.cs (new)
Injected with FantasyFootballDbContext; registered in Program.cs DI. Deterministic, seed-based so the previewed result is exactly what gets committed (no heavy draft storage — only an int seed is passed from preview to confirm).

SimulationDraft GenerateDraft(Gameweek gw, int seed) — pure, no DB writes
Returns in-memory fixtures + performances (with computed points):

var rng = new Random(seed); (all randomness from here ⇒ reproducible).
Distinct clubs from Players.Club (20). Shuffle, pair into 10 fixtures (home/away).
Per fixture roll a random scoreline weighted toward low scores (e.g. goals ~ {0:25%,1:30%,2:22%,3:13%,4:6%,5:3%,6:1%}). Each side: conceded = other side's goals, cleanSheetEligible = conceded == 0.
Per club pick a starting XI from its 15 players (1 GK + 4 DEF + 4 MID + 2 FWD). Assign minutes (most 90; a couple 45–70). Distribute that club's goals to scorers weighted FWD>MID>DEF>GK; ~60% of goals add an assist from a different teammate. Random cards (yellow ~12%, red ~1.5%). GK Saves random (more when conceding more). Set GoalsConceded on GK+DEF; CleanSheet = (conceded==0 && minutes>=60).
Compute PointsEarned per FPL rules (§3a). Bonus: within each fixture rank by base points, award +3/+2/+1 to the top three (added to points).
Task ConfirmAsync(int gameweekId, int seed)
Load the (Pending) gameweek; GenerateDraft(gw, seed).
Persist Fixture rows and MatchPerformance rows (set Opponent, MatchDate, GameweekId, PlayerId).
Aggregate onto players: TotalPoints += pts, Goals/Assists/CleanSheets +=.
Per FantasyTeam: sum its 11 starters' points this GW (from StartingLineupIds or default XI) → create GameweekTeamScore (with LineupIds snapshot); FantasyTeam.TotalPoints += that.
SaveChanges.
Task DeleteWithReversalAsync(int gameweekId) (used by Delete action)
Load gameweek Include(Performances) + its GameweekTeamScores.
Subtract performances from Player.TotalPoints/Goals/Assists/CleanSheets; subtract GameweekTeamScore.Points from FantasyTeam.TotalPoints.
Remove(gameweek) (cascade removes Performances, Fixtures, scores); save.
§3a FPL scoring (from FPL-Scoring-Rules.md)
Per performance (pos, min, gc, saves):

Appearance: min>=60 → +2, 1–59 → +1, 0 → 0.
Goals × {GK 10, DEF 6, MID 5, FWD 4}; Assists × 3.
Clean sheet (only if min>=60 && gc==0): GK/DEF +4, MID +1, FWD 0.
Saves (GK): + floor(saves/3). Conceded (GK/DEF): - floor(gc/2).
Yellow −1 each; Red −3 each; Bonus +0..3.
(Penalty save/miss & own goals omitted for simplicity — note in code.) PointsEarned may be negative (valid in FPL).

4. Gameweek controller & Details — Controllers/GameweekController.cs
Keep Index (/kola), Create (GET/POST), Edit (GET/POST), Search, and the whole Gameweeks-tab UI unchanged. The Api/GameweekApiController stays intact. Inject GameweekSimulationService.

Details (/kolo/{id}): load the gameweek with Fixtures and Performances (ensure GameweekRepository.GetById includes both). Pass a small GameweekDetailsViewModel { Gameweek, Fixtures, IsSimulated, CanSimulate } (CanSimulate = IsAdmin && !IsSimulated).
New Simulate (POST, [Authorize(Roles=AdminRole)], antiforgery): pick a random seed, GenerateDraft(gw, seed), and return a preview view (SimulatePreview.cshtml) — nothing saved. The view shows the 10 fixtures + performances/points and carries the seed in hidden fields with Confirm, Re-roll (re-POST Simulate), and Cancel (back to Details) actions.
New Confirm (POST, admin, antiforgery): ConfirmAsync(gameweekId, seed), TempData success, redirect to Details (now shows committed stats).
Delete (POST): replace the current "blocked if performances exist" logic with DeleteWithReversalAsync so admins can remove/redo a simulated gameweek and have its points reversed. (Delete confirmation page stays.)
Views (ux-designer)
Views/Gameweek/Details.cshtml (modify): add a Fixtures section listing the 10 scorelines (HomeClub H–A AwayClub); add Saves/Conceded/Bonus columns to the performances table; keep the existing position filter/sort. For a Pending gameweek: admins see a "Simuliraj rezultate" button (POST → Simulate); everyone else sees a "nije još odigrano" empty state.
Views/Gameweek/SimulatePreview.cshtml (new): themed preview of the rolled fixtures + performances + points; Potvrdi / Ponovno simuliraj / Odustani buttons (seed in hidden field).
Views/Gameweek/Index.cshtml: unchanged structurally; optional small status badge (Pending vs Played) per card based on Fixtures.Any().
(optional) redirect Create POST to the new gameweek's Details so the admin can simulate immediately.

5. My Team gameweek slider
Controllers/FantasyTeamController.cs — MyTeam(int? gw = null)
Load all simulated gameweeks (number+id) for the slider.
If gw set: load MatchPerformances for the squad's players in that gameweek → Dictionary<playerId,int> GW points; load the team's GameweekTeamScore for that GW → headline team score.
Feed these into BuildMyTeamViewModel.
Models/ViewModels/MyTeamViewModel.cs
Add: List<(int Number,int Id)> Gameweeks, int? SelectedGameweek, int? GameweekTeamPoints, Dictionary<int,int> PlayerGameweekPoints, int SeasonTotalPoints.

Views/FantasyTeam/MyTeam.cshtml + Views/Shared/_PitchPlayer.cshtml (ux-designer)
Slider / ◀ ▶ arrows above the pitch: positions = Season (default) + GW1…GWn; themed, keyboard-accessible, ARIA-labelled.
Season: shirts show TotalPoints, header shows season total.
A GW selected: shirts show that player's GW points (PlayerGameweekPoints, 0 if they didn't feature); prominent header "Gameweek N: {GameweekTeamPoints} pts" (the authoritative snapshot).
_PitchPlayer.cshtml gains an optional GW-points mode via ViewData (falls back to TotalPoints). Keep the current squad on the pitch; note in code that after transfers the per-shirt overlay may not sum exactly to the banked headline (expected).

6. Other touch points
Program.cs: register GameweekSimulationService in DI.
Home page (HomeController / _TotwContent): no change required — already guarded for "no gameweeks" (PopulateTotw early-returns; widgets check latest != null). TOTW / Player-of-the-Week populate from simulated data after the first confirmed gameweek.
FantasyTeam delete path: since GameweekTeamScore→FantasyTeam is Restrict, the team-delete action must clear that team's GameweekTeamScores first.
GameweekRepository.GetById: include Fixtures (and keep Performances→Player) so Details renders both.
Files at a glance
New: Models/Fixture.cs, Models/GameweekTeamScore.cs, Services/GameweekSimulationService.cs, Models/ViewModels/GameweekDetailsViewModel.cs, Models/ViewModels/SimulatePreviewViewModel.cs, Views/Gameweek/SimulatePreview.cshtml, + 1 EF migration. Modified: Models/Gameweek.cs, Models/MatchPerformance.cs, Models/ViewModels/MyTeamViewModel.cs, DAL/FantasyFootballDbContext.cs, DAL/DbSeeder.cs, Repositories/PlayerMockRepository.cs (prices), Controllers/GameweekController.cs, Controllers/FantasyTeamController.cs, Repositories/GameweekRepository.cs, Views/Gameweek/Details.cshtml, Views/Gameweek/Index.cshtml, Views/FantasyTeam/MyTeam.cshtml, Views/Shared/_PitchPlayer.cshtml, Program.cs, wwwroot/css/site.css, wwwroot/js/site.js. Deleted: none (Create/Edit/Delete stay).

7. Verification
Build (net9 app; SDK available). Apply migration via app startup.
Reset check: Players tab — every player shows 0 pts/goals; prices match player-prices.md (Haaland £14.7, Salah £14.0, Gabriel £7.3, fringe ≈£4.0–4.5); Gameweeks tab empty; team building still works within the £100 budget.
Create + simulate (log in as admin/demo account — first user gets Admin): "Kreiraj kolo" → open the new Gameweek card → Simuliraj rezultate → preview shows 10 fixtures + performances + points; re-roll changes them; Confirm.
Everyone sees stats: re-open the gameweek (as a normal user) → Details shows the 10 fixture results + the performances table; some players'/teams' TotalPoints are now > 0; Home shows Player/Team of the Week.
My Team slider: open My Team, slide to that GW → "Gameweek N: X pts" header + per-player GW points; slide back to Season → totals.
Sequential: create + simulate a second gameweek; verify the first is unchanged and the slider shows both.
Delete/undo: Delete a simulated gameweek → it disappears and players'/teams' points revert.
MCP spot-check: list_gameweeks, list_players confirm persisted data.
Reminder: all .cshtml/CSS/JS work in §4–§5 is delegated to the ux-designer subagent per CLAUDE.md.