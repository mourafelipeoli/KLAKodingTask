# AiCallQueries

The month-to-date AI spend (sum of `CostMicroUsd` since the first day of the current UTC
month). Shared by the AI budget guard and the infrastructure page, so both always show the
same number. The nullable cast makes an empty month sum to zero instead of failing.
