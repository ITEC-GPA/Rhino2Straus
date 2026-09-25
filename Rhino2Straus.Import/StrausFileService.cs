using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Rhino.Geometry;
using Rhino2Straus.Api;
using St7API;

namespace Rhino2Straus.Import;

public sealed record StrausNode(int Number, int Id, Point3d Position);
public sealed record StrausElement(int Number, int Id, int Property, int[] Nodes);
public sealed record StrausGeometry(StrausNode[] Nodes, StrausElement[] Beams, StrausElement[] Plates, int[] Units, string[] LoadCases, StrausElement[] Solids);
public sealed record ResultTable(double[] Positions, double[][] Values);

public static partial class StrausFileService
{
    private static void Check(int code, string operation) => StrausRuntime.Check(code, operation);

    public static StrausGeometry ReadGeometry(string path)
    {
        using var session = new StrausSession(path);
        int id = session.Id;
        var units = new int[St7.kLastUnit];
        Check(St7.St7GetUnits(id, units), "Read units");
        int total = 0;
        Check(St7.St7GetTotal(id, St7.tyNODE, ref total), "Read node count");
        var nodes = new List<StrausNode>();
        for (int n = 1; n <= total; n++)
        {
            var xyz = new double[3]; int label = 0;
            Check(St7.St7GetNodeXYZ(id, n, xyz), $"Read node {n}");
            Check(St7.St7GetNodeID(id, n, ref label), $"Read node ID {n}");
            nodes.Add(new StrausNode(n, label, new Point3d(xyz[0], xyz[1], xyz[2])));
        }
        StrausElement[] Elements(int kind)
        {
            int count = 0;
            Check(St7.St7GetTotal(id, kind, ref count), "Read element count");
            var elements = new List<StrausElement>();
            for (int n = 1; n <= count; n++)
            {
                var connection = new int[St7.kMaxElementNode + 1]; int property = 0, label = 0;
                Check(St7.St7GetElementConnection(id, kind, n, connection), $"Read element {n}");
                Check(St7.St7GetElementProperty(id, kind, n, ref property), $"Read property of element {n}");
                Check(kind == St7.tyBEAM ? St7.St7GetBeamID(id, n, ref label) : kind == St7.tyPLATE ? St7.St7GetPlateID(id, n, ref label) : St7.St7GetBrickID(id,n,ref label), "Read element ID");
                if (connection[0] < 1 || connection[0] > St7.kMaxElementNode) throw new InvalidDataException("Invalid element connectivity.");
                elements.Add(new StrausElement(n, label, property, connection.Skip(1).Take(connection[0]).ToArray()));
            }
            return elements.ToArray();
        }
        int caseCount = 0;
        Check(St7.St7GetNumLoadCase(id, ref caseCount), "Read load cases");
        var cases = new string[caseCount];
        for (int c = 1; c <= caseCount; c++)
        {
            var name = new StringBuilder(1024);
            Check(St7.St7GetLoadCaseName(id, c, name, name.Capacity), "Read load case name");
            cases[c - 1] = name.ToString();
        }
        return new StrausGeometry(nodes.ToArray(), Elements(St7.tyBEAM), Elements(St7.tyPLATE), units, cases, Elements(St7.tyBRICK));
    }

    public static string[] ResultCases(string model, string result)
    {
        using var session = new StrausSession(model);
        var counts = session.OpenResults(result);
        var names = new string[counts.Primary + counts.Secondary];
        for (int c = 1; c <= names.Length; c++)
        {
            var name = new StringBuilder(1024);
            Check(St7.St7GetResultCaseName(session.Id, c, name, name.Capacity), "Read result case");
            names[c - 1] = name.ToString();
        }
        return names;
    }

    private static StrausSession Results(string model, string result, int @case)
    {
        var session = new StrausSession(model);
        try
        {
            var counts = session.OpenResults(result);
            if (@case < 1 || @case > counts.Primary + counts.Secondary) throw new ArgumentOutOfRangeException(nameof(@case), "Result case is outside the result file range.");
            return session;
        }
        catch { session.Dispose(); throw; }
    }

    public static double[][] NodeResults(string model, string result, int @case, IEnumerable<int> numbers, bool reactions)
    {
        using var session = Results(model, result, @case);
        return numbers.Select(n => {
            if (n < 1) throw new ArgumentOutOfRangeException(nameof(numbers), "Use Straus7 node numbers, starting at 1.");
            var values = new double[6];
            Check(St7.St7GetNodeResult(session.Id, reactions ? St7.rtNodeReact : St7.rtNodeDisp, n, @case, values), $"Read node {n} results");
            return values;
        }).ToArray();
    }

    public static ResultTable BeamResults(string model, string result, int @case, int number, int quantity, int axes, int stations)
    {
        if (number < 1 || stations < 2 || stations > 100) throw new ArgumentException("Beam number must be positive; use between 2 and 100 stations.");
        using var session = Results(model, result, @case);
        var values = new double[St7.kMaxBeamResult];
        var positions = new double[St7.kMaxBeamResult];
        Check(St7.St7SetBeamResultPosMode(session.Id, St7.bpLength), "Set beam station distance mode");
        int rows = 0, columns = 0;
        Check(St7.St7GetBeamResultArray(session.Id, quantity, axes, number, stations, @case, ref rows, ref columns, positions, values), "Read beam results");
        return new ResultTable(positions.Take(rows).ToArray(), Split(values, rows, columns));
    }

    public static ResultTable PlateResults(string model, string result, int @case, int number, int quantity, int surface, int sample)
    {
        if (number < 1 || surface < 0 || surface > 2 || sample < 0 || sample > 4) throw new ArgumentException("Invalid plate number, sample location or surface.");
        using var session = Results(model, result, @case);
        var values = new double[St7.kMaxPlateResult]; int rows = 0, columns = 0;
        Check(St7.St7GetPlateResultArray(session.Id, quantity, St7.stPlateLocal, number, @case, sample, surface, 0, ref rows, ref columns, values), "Read plate results");
        return new ResultTable(Enumerable.Range(0, rows).Select(x => (double)x).ToArray(), Split(values, rows, columns));
    }

    public static double[][] Split(double[] buffer, int rows, int columns)
    {
        if (rows < 0 || columns < 0 || (long)rows * columns > buffer.Length) throw new InvalidDataException("Straus7 returned an invalid result array size.");
        return Enumerable.Range(0, rows).Select(i => buffer.Skip(i * columns).Take(columns).ToArray()).ToArray();
    }

    public static ResultTable SolidResults(string model, string result, int @case, int number, int quantity, int sample, bool combined)
    {
        if(number<1||sample<0||sample>4)throw new ArgumentException("Invalid solid number or sampling location.");
        using var session=Results(model,result,@case);
        var buffer=new double[St7.kMaxBrickResult];int rows=0,columns=0;
        Check(St7.St7GetBrickResultArray(session.Id,quantity,combined?St7.stBrickCombined:St7.stBrickGlobal,number,@case,sample,ref rows,ref columns,buffer),"Read solid results");
        return new ResultTable(Enumerable.Range(0,rows).Select(x=>(double)x).ToArray(),Split(buffer,rows,columns));
    }

    public static string RunLinearStatic(string path, bool overwriteResults)
        => RunAnalysis(path, ".LSA", St7.stLinearStatic, overwriteResults, _ => { });
}
