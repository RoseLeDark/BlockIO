# Contributing to BlockIO

Thank you for your interest in contributing to BlockIO!  
This project is dedicated to open, modular tooling for block-level inspection and repair on Windows.

## Philosophy

BlockIO is strictly structural.  
It does **not** interpret filesystem contents and does **not** include logic for FAT, NTFS, or other filesystems.  
All contributions must respect this boundary.

## How to Contribute

- Fork the repository and create a feature branch
- Follow the architectural style: modular, introspective, and plugin-friendly
- Document all public APIs with XML comments
- Write clear commit messages and include tests if applicable
- Submit a pull request with a description of your changes

## Code Style

- Use C# 10.0 or higher
- Prefer explicit over implicit logic
- Avoid magic numbers, hidden defaults, or silent fallbacks
- Use `Span<T>`, `Memory<T>`, and `FileStream` directly — no wrappers unless justified

## Plugin Development

BlockIO supports plugins via conditional compilation and interface discovery.  
If you're building a plugin, please:

- Use `BLOCKIO_PLUGIN_SUPPORT` as a conditional define
- Keep plugin logic isolated and introspectable
- Avoid filesystem interpretation — use FileSysIO for that

## Reporting Issues

Use GitHub Issues for bugs, feature requests, or architectural discussions.  
Security issues should be reported privately — see [SECURITY.md](SECURITY.md).

## License

By contributing, you agree that your code will be released under the [EUPL v1.2](https://eupl.eu/1.2/en/).
