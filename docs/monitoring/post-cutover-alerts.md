# Post-Cutover Monitoring (30 days)

Monitor production for 30 consecutive days after PHP cutover (US-110).

## Metrics & alerts

| Signal | Threshold | Action |
|--------|-----------|--------|
| API 5xx rate | > 1% for 15 min | Page on-call |
| API p95 latency | > 2× baseline for 30 min | Investigate DB/API |
| Daily sales count | > 1% deviation vs 14-day baseline for 24h | Review Sales BC logs |
| Stock discrepancy job | Any non-zero | Halt cutover sign-off |
| Digest/report job failures | Any failure in 24h | Check Notifications/Reporting |

## Rollback trigger

If a **critical business metric** (daily sales total, open receivables aggregate) deviates **> 1% for 24 hours** after cutover, initiate rollback per `docs/runbooks/php-decommission.md`.

## Dashboard checklist

- [ ] API health `/health` uptime
- [ ] Error rate from application logs
- [ ] `migration_parallel_run_stats` empty or frozen post cutover
- [ ] Zero tenants on PHP path in migration status

## Sign-off

After 30 days without rollback, archive EP-011 and retain backups per retention policy.
