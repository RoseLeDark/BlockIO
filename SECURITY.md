Security Policy
Supported Versions
BlockIO is currently under active development. The latest tagged release is supported.
Older versions may receive critical fixes at the maintainer's discretion

| Version | Supported |
|---------|-----------|
| 0.4.3   | ✅ Yes     |
| <0.4.0  | ❌ No      |

Perfect — here's an extended version of your SECURITY.md that includes the architectural safety principles you've implemented, especially around deterministic control flow, lock safety, and task compatibility. This version preserves your current structure and tone, while adding a new section called "Architectural Safety Principles":

Security Policy
Supported Versions
BlockIO is currently under active development. The latest tagged release is supported.
Older versions may receive critical fixes at the maintainer's discretion.
|  |  | 
|  |  | 
|  |  | 


Reporting a Vulnerability
If you discover a security issue in BlockIO, please report it privately.
- Email: ambersophia.schroeck@gmail.com
- GitHub: Open a private security advisory
Please include:
- A clear description of the issue
- Steps to reproduce (if applicable)
- A proposed mitigation or patch (optional but appreciated)
We aim to respond within 5 working days.
Scope
BlockIO operates strictly at the block structure level.
It does not interpret filesystem contents or expose user-level data.
Security issues should relate to:
- Unsafe parsing of GPT/MBR structures
- Buffer overflows or unsafe memory access
- Plugin interface vulnerabilities
- Misuse of low-level device access
Out of Scope
The following are not considered security issues in BlockIO:
- Filesystem logic (handled by FileSysIO)
- Interpretation errors in external plugins
- Device-specific quirks outside BlockIO's abstraction
Architectural Safety Principles
BlockIO enforces a set of architectural patterns that support safe, deterministic, and task-compatible behavior. These are critical for future integration with parallel processing, lock-sensitive environments, and real-time systems.
✅ Unified Return Path (RTSafePattern)
All control flows in critical methods use a single return point via a dedicated return variable (e.g. _ret). This avoids early return statements and ensures:
- Deterministic execution
- Safe lock release and resource cleanup
- Traceable error handling
- Compatibility with task schedulers and RTOS-like environments


```
bool _ret = false;
try {
    // Operation
    _ret = true;
} catch (Exception ex) {
    err = ex;
}
return _ret;
```

✅ DryRun Support
All write-capable operations respect a DryRun flag, allowing safe simulation without modifying the underlying device. This is essential for:
- UI previews
- Testing environments
- Forensic workflows
✅ Explicit Error Propagation
Methods like TryReadGptHeader(out, ref) provide structured error reporting without relying on exceptions. This supports:
- Predictable control flow
- External diagnostics
- Safe integration into multi-tasking environments
✅ Sector-Level Validation
All read/write operations enforce strict alignment with SectorSize. Misaligned buffers are rejected early to prevent:
- Partial writes
- Memory corruption
- Device inconsistencies

Philosophy
BlockIO is designed for transparency, modularity, and user empowerment.
Security is treated as a structural concern — not as an afterthought.
We welcome responsible disclosure and community collaboration.
