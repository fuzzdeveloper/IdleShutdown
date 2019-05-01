# IdleShutdown
Shuts down a Windows PC after a set length of idle time. Made to be used on
systems with broken sleep functionality - desktop's with certain Gigabyte
motherboards (like mine) for example.

The app checks both the users last input and the SystemExecutionState (the
latter is used to avoid shutting down when user is playing a movie, when a
network share is in use, etc).

The app shut's down via shutdown.exe and performs a shutdown with configurable
delay - if shutdown is aborted (via "shutdown.exe /a") the app will resume
detection of idle (and perform subsequent shutdown call if detected).

This app is suitable for starting via startup entry or via task scheduler (use
login as trigger - this app cannot be run using windows startup as trigger).

## Example usage:
IdleShutdown takes two parameters - the first is how many seconds of idle are
needed before shutdown, and the second is the shutdown timer value.

The following usage will have it wait for 2 hours of idle before shutdown,
then shutdown with 2 minute delay:
```
IdleShutdown.exe 7200 120
```
