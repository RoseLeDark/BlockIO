DEVELOPMENT.md — Architectural Development Principles for BlockIO
Purpose
This document outlines the architectural programming principles used in BlockIO.
It complements SECURITY.md and CONTRIBUTING.md by describing how BlockIO is structured, how control flow is handled, and how contributors should approach low-level logic.
BlockIO is not a typical C# application — it is a structural toolkit for block-level inspection and repair.
Its design reflects principles from real-time systems, forensic tooling, and modular architecture.

Core Principles
🧱 Structural, Not Interpretive
BlockIO operates strictly at the block structure level.
It does not interpret filesystem contents (e.g. FAT, NTFS) and does not expose user-level data.
All logic must respect this boundary.
🧭 Deterministic Control Flow (RTSafePattern)
Critical methods use a unified return path via a dedicated return variable (e.g. _ret).
Early return statements are avoided to ensure:
- Predictable execution
- Safe lock release and cleanup
- Compatibility with task schedulers and real-time environments
- Easier debugging and tracing
Example:

````
bool _ret = false;
try {
    // Operation
    _ret = true;
} catch (Exception ex) {
    err = ex;
}
return _ret;

````
his pattern is mandatory in all device-level and write-capable methods.
🧪 DryRun Support
All write-capable operations must respect a DryRun flag.
This allows safe simulation, previewing, and testing without modifying the underlying device.
DryRun is enforced in:
- GPT layout creation
- Sector writes
- Plugin operations (if applicable)
🧠 Explicit Error Propagation
Methods like TryReadGptHeader(out, ref) provide structured error reporting without relying on exceptions.
This supports:
- Predictable control flow
- External diagnostics
- Safe integration into multi-tasking environments
Avoid throw unless absolutely necessary — prefer ref Exception or out ErrorInfo.
📏 Sector-Level Validation
All read/write operations must validate buffer alignment with SectorSize.
Misaligned buffers must be rejected early to prevent:
- Partial writes
- Memory corruption
- Device inconsistencies

Recommended Practices
- Use Span<T> and Memory<T> for buffer operations
- Avoid hidden defaults, magic numbers, or silent fallbacks
- Keep plugin logic introspectable and isolated
- Prefer TryX(out, ref) over exception-based parsing
- Document all public APIs with XML comments

Anti-Patterns (Do Not Use)
- Filesystem interpretation inside BlockIO (use FileSysIO)
- Early return in critical methods
- Silent error swallowing
- Implicit behavior based on device type or OS quirks
- Wrappers around FileStream unless justified

Future Compatibility
These principles are designed to support:
- Task-based parallelism
- Lock-safe device access
- Real-time inspection tools
- Forensic workflows
- Plugin-based extensibility

