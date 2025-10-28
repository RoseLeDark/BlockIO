# Security Policy

## Supported Versions

BlockIO is currently under active development. The latest tagged release is supported.  
Older versions may receive critical fixes at the maintainer's discretion.

| Version | Supported |
|---------|-----------|
| 0.4.3   | ✅ Yes     |
| <0.4.0  | ❌ No      |

## Reporting a Vulnerability

If you discover a security issue in BlockIO, please report it privately.

- **Email**: ambersophia.schroeck@gmail.com  
- **GitHub**: [Open a private security advisory](https://github.com/RoseLeDark/BlockIO/security/advisories)

Please include:

- A clear description of the issue
- Steps to reproduce (if applicable)
- A proposed mitigation or patch (optional but appreciated)

We aim to respond within **5 working days**.

## Scope

BlockIO operates strictly at the block structure level.  
It does **not** interpret filesystem contents or expose user-level data.  
Security issues should relate to:

- Unsafe parsing of GPT/MBR structures
- Buffer overflows or unsafe memory access
- Plugin interface vulnerabilities
- Misuse of low-level device access

## Out of Scope

The following are **not** considered security issues in BlockIO:

- Filesystem logic (handled by FileSysIO)
- Interpretation errors in external plugins
- Device-specific quirks outside BlockIO's abstraction

## Philosophy

BlockIO is designed for transparency, modularity, and user empowerment.  
Security is treated as a structural concern — not as an afterthought.

We welcome responsible disclosure and community collaboration.
