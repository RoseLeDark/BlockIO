using System;
using System.Collections.Generic;
using System.IO;
using Xunit;
using BlockIO.GPT;

namespace BlockIO.Tests.GPT
{
    public class GptManifestTests
    {
        [Fact]
        public void ParseAndValidateYamlManifest_ShouldSucceed()
        {
            // Arrange: Beispiel-YAML mit zwei gültigen Partitionen
            string yaml = """
            GptEntryArray:
              - Index: 0
                GptEntry:
                  TypeGuid: 0FC63DAF-8483-4772-8E79-3D69D8477DE4
                  UniqueGuid: 11111111-2222-3333-4444-555555555555
                  FirstLBA: 2048
                  LastLBA: 4095
                  Attributes: 0
                  Name: "Linux Root"
              - Index: 1
                GptEntry:
                  TypeGuid: 0FC63DAF-8483-4772-8E79-3D69D8477DE4
                  UniqueGuid: 66666666-7777-8888-9999-AAAAAAAAAAAA
                  FirstLBA: 4096
                  LastLBA: 8191
                  Attributes: 0
                  Name: "Linux Home"
            """;

            // Act: Manifest parsen und validieren
            var array = GptEntryArray.FromYaml(yaml);
            var errors = GptManifestValidator.Validate(array);

            // Assert: Keine Fehler erwartet
            Assert.NotNull(array);
            Assert.Equal(2, array.Count);
            Assert.Empty(errors);
        }

        [Fact]
        public void ParseYamlManifest_WithDuplicateGuid_ShouldFail()
        {
            // Arrange: YAML mit doppeltem UniqueGuid
            string yaml = """
            GptEntryArray:
              - Index: 0
                GptEntry:
                  TypeGuid: 0FC63DAF-8483-4772-8E79-3D69D8477DE4
                  UniqueGuid: DDDDDDDD-DDDD-DDDD-DDDD-DDDDDDDDDDDD
                  FirstLBA: 2048
                  LastLBA: 4095
                  Attributes: 0
                  Name: "Root"
              - Index: 1
                GptEntry:
                  TypeGuid: 0FC63DAF-8483-4772-8E79-3D69D8477DE4
                  UniqueGuid: DDDDDDDD-DDDD-DDDD-DDDD-DDDDDDDDDDDD
                  FirstLBA: 4096
                  LastLBA: 8191
                  Attributes: 0
                  Name: "Home"
            """;

            // Act
            var array = GptEntryArray.FromYaml(yaml);
            var errors = GptManifestValidator.Validate(array);

            // Assert
            Assert.NotEmpty(errors);
            Assert.Contains(errors, e => e.Contains("Duplicate UniqueGuid"));
        }
    }
}
