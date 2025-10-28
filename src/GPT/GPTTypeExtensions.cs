// SPDX-License-Identifier: EUPL-1.2
// This file is part of the BlockIO project.
// Copyright © 2025 Amber-Sophia Schröck <ambersophia.schroeck@gmail.com>

using System;
using System.Collections.Generic;
using System.Text;

namespace BlockIO.GPT
{
    public static class GPTTypeExtensions
    {
        public static GPTType? GetUniqueType(this Guid gptGuid)
        {
            foreach (var kvp in GPTTypeRegistry.TypeGuids)
            {
                if (kvp.Value.Equals(gptGuid))
                    return kvp.Key;
            }
            return null;
        }

        public static Guid? GetGuid(this GPTType type)
        {
            if (type == GPTType.UserDefined)
                return null;
            return GPTTypeRegistry.TypeGuids.TryGetValue(type, out var guid) ? guid : null;
        }

        public static string GetTypeName(this Guid gptGuid)
        {
            foreach (var kvp in GPTTypeRegistry.TypeGuids)
            {
                if (kvp.Value.Equals(gptGuid))
                    return kvp.Key.ToString();
            }

            var userName = GPTTypeRegistry.GetUsertName(gptGuid);
            return userName ?? $"Unknown ({gptGuid})";
        }

    };
}
