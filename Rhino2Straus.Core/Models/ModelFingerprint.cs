using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text.Json;
using Rhino.Geometry;
using Rhino2Straus.Core.Attributes;

namespace Rhino2Straus.Core.Models;

/// <summary>Identity of the analysis input graph. Display Breps are excluded, but their effective insertion offsets are included.</summary>
public static class ModelFingerprint
{
    public static string Compute(ModelModel model)
    {
        ArgumentNullException.ThrowIfNull(model);
        using var stream = new MemoryStream();
        using (var json = new Utf8JsonWriter(stream))
        {
            var stack = new HashSet<object>(ReferenceEqualityComparer.Instance);
            json.WriteStartArray();
            // Export contract v2: beam load B is a Model end position, converted per native schema.
            json.WriteStringValue("Rhino2Straus.input.v2");
            foreach (var item in new object[] { model.ModelUnits, model.SelfWeight,
                model.NodeElements.Values.OrderBy(x => x.Id), model.FrameElements.Values.OrderBy(x => x.Id), model.AreaElements.Values.OrderBy(x => x.Id),
                model.SolidElements.Values.OrderBy(x => x.Id), model.LinkElements.Values.OrderBy(x => x.Id), model.Materials.Values.OrderBy(x => x.Id),
                model.FrameProperties.Values.OrderBy(x => x.Id), model.AreaProperties.Values.OrderBy(x => x.Id), model.LoadCases.Values.OrderBy(x => x.Name, StringComparer.Ordinal),
                model.LoadCombinations.Values.OrderBy(x => x.Name, StringComparer.Ordinal), model.Groups.Values.OrderBy(x => x.GroupFullName, StringComparer.Ordinal) }) Write(json, item, stack);
            json.WriteEndArray();
        }
        return Convert.ToHexString(SHA256.HashData(stream.ToArray()));
    }

    private static void Write(Utf8JsonWriter json, object value, HashSet<object> stack)
    {
        if (value is null) { json.WriteNullValue(); return; }
        var type = value.GetType();
        if (value is string text) { json.WriteStringValue(text); return; }
        if (type.IsPrimitive || type.IsEnum || value is decimal)
        { json.WriteStringValue(Convert.ToString(value, System.Globalization.CultureInfo.InvariantCulture)); return; }
        if (value is Point3d p) { Write(json, new[] { p.X, p.Y, p.Z }, stack); return; }
        if (value is Point2d p2) { Write(json, new[] { p2.X, p2.Y }, stack); return; }
        if (value is Vector3d v) { Write(json, new[] { v.X, v.Y, v.Z }, stack); return; }
        if (value is System.Drawing.Color colour) { json.WriteNumberValue(colour.ToArgb()); return; }
        if (!type.IsValueType && !stack.Add(value)) throw new InvalidOperationException("Cycle in the model input graph.");
        try
        {
            if (value is IEnumerable items)
            {
                json.WriteStartArray(); foreach (var item in items) Write(json, item, stack); json.WriteEndArray();
            }
            else
            {
                json.WriteStartObject(); json.WriteString("$type", type.FullName);
                if(value is Rhino2Straus.Core.Elements.FrameElementModel beam)
                {
                    json.WritePropertyName("$effectiveOffset");Write(json,beam.GetEffectiveOffset(),stack);
                }
                foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .Where(p => p.CanRead && p.CanWrite && p.GetIndexParameters().Length == 0)
                    .OrderBy(p => p.Name, StringComparer.Ordinal))
                {
                    if (property.Name is "Breps" or "Brep" or "Results") continue;
                    if (value is ElementGroupModel && property.Name.EndsWith("ElementList", StringComparison.Ordinal)) continue;
                    json.WritePropertyName(property.Name); Write(json, property.GetValue(value), stack);
                }
                json.WriteEndObject();
            }
        }
        finally { if (!type.IsValueType) stack.Remove(value); }
    }
}
