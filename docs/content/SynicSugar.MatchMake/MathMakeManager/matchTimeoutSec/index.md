+++
title = "matchTimeoutSec"
weight = 1
+++
## matchTimeoutSec
int matchTimeoutSec = 180;

### Description
The second of host user to close lobby on waiting for other users. This start counting after the lobby is created.
 After this time has passed without other user joining, matching method returns false, and the matchmake fails.
default is 180