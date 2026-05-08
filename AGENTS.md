# BattleDemo Project Rules

## Logging
- Never use `Debug.Log`, `Debug.LogWarning`, or `Debug.LogError` directly.
- Always use `HLog` from `Tools.Log` namespace instead:
  - `Debug.Log` → `HLog.Log`
  - `Debug.LogWarning` → `HLog.LogW`
  - `Debug.LogError` → `HLog.LogE`
  - Use the owner overload `HLog.Log(this, ...)` when inside a class for better context.
