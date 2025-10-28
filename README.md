# BlockIO – Low-Level Disk Access for C# on Windows

**BlockIO** is a C# library that fills a critical gap in the Windows ecosystem: it enables direct, low-level access to physical disks and partitions — without relying on filesystem APIs or unsafe native code. Inspired by Linux tools like `dd`, `partx`, and `blkid`, BlockIO gives .NET developers structured, introspectable control over block devices.

---

## 🧩 Purpose

Windows lacks a clean, managed way to access raw disk sectors, GPT/MBR headers, and partition boundaries. BlockIO solves this by offering:

- Safe, stream-based access to physical disks and partitions
- GPT and MBR parsing without filesystem dependency
- Partition introspection via GUID, name, or index
- Device abstraction for RAM, files, USB, and virtual disks
- Compression-ready containers for snapshots and transfer

---

## 🔧 Core Components

### 📦 Devices

- `AbstractDevice` – base class for all device types
- `FileDevice`, `RamDevice`, `UsbDevice`, `VirtualDiskDevice`
- `CreateDeviceStream()` for raw access
- `GetPartitionById`, `GetPartitionByGuid`, `GetPartitionByName`

### 🧱 Partitions

- `AbstractPartition` – holds LBA range, GUID, name
- `CreateStream()` – returns a bounded stream over the partition
- No filesystem logic — structure only

### 🔄 Streams

- `PartitionStream`, `DeviceStream`
- Access variants:
  - `ReadOnlyPartitionStream`
  - `WritePartitionStream`
  - `RWPartitionStream`
  - Same for device-level streams
- All implement `IBlockStream` with:
  ```csharp
  Stream CloneAsReadOnly();
  Stream CloneAsWriteOnly();
  Stream CloneAsReadWrite();

### 🧠 Utilities

HeaderUtility – GPT/MBR header parsing and validation

BlockRange, StreamTraits, DeviceDescriptor – for analysis and tooling


### 🚀 Example

var device = new Device("\.\\..", new GPTParser(), true);
var partition = device.GetPartitionById(0);
using var stream = partition.CreateStream(FileAccess.ReadWrite); // raw access to partition

### 🧠 Philosophy

BlockIO is not a filesystem library. It does not mount, interpret, or parse FAT, NTFS, ext4, or any other FS. It exposes structure, not content.


