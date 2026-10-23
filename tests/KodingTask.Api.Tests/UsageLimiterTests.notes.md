# UsageLimiterTests

The limiter's contract: exactly `limit` grants per counter per UTC day, a closed limit never
grants, counters are independent, the day rolls over at midnight UTC (the clock starts one
minute before it), and two contexts sharing the database cannot both take the last unit.
