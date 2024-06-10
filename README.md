# Event Status
A CS2 Plugin Made to log the amount time each player spent active during an event for the [EdgeGamers](EdgeGamers.Com) Events Server. Repository Made for me to track changes.
## Description
Plugin accurately tracks the amount of time plaer spends on the server.
- Will stop timer when a player leaves
- Will pick up where the timer left off if that same player rejoins
- Will Track across map changes
- Ignores Bots
- Tracks Peak Player count during the event
## Requirements
- [MetaMod:Source](https://github.com/alliedmodders/metamod-source/)
- [CounterStrikeSharp](https://github.com/roflmuffin/CounterStrikeSharp)
## Usage
- css_estart: Starts the event and begins tracking player times.
- css_estatus: Displays the current status of the event, including the duration and the time each player has spent on the server.
- css_estop: Stops the event and clears all tracked data.
### Sample Output
```
=== Begin Event Logs (Duration: 1.42 hours) - (Peak: 30 players) ===
1. 12345678901234567 PlayerOne 1.40 hours
2. 23456789012345678 PlayerTwo 1.35 hours
3. 34567890123456789 PlayerThree 1.30 hours
4. 45678901234567890 PlayerFour 1.25 hours
5. 56789012345678901 PlayerFive 1.20 hours
6. 67890123456789012 PlayerSix 1.15 hours
7. 78901234567890123 PlayerSeven 45.20 minutes
8. 89012345678901234 PlayerEight 38.50 minutes
9. 90123456789012345 PlayerNine 27.15 minutes
10. 01234567890123456 PlayerTen 20.30 minutes
11. 11234567890123457 PlayerEleven 15.45 minutes
12. 21234567890123458 PlayerTwelve 10.10 minutes
13. 31234567890123459 PlayerThirteen 5.25 minutes
14. 41234567890123450 PlayerFourteen 4.15 minutes
15. 51234567890123451 PlayerFifteen 3.45 minutes
16. 61234567890123452 PlayerSixteen 3.00 minutes
17. 71234567890123453 PlayerSeventeen 2.30 minutes
18. 81234567890123454 PlayerEighteen 1.45 minutes
19. 91234567890123455 PlayerNineteen 1.00 minute
20. 10123456789012346 PlayerTwenty 45 seconds
=== End Event Logs ===
```
