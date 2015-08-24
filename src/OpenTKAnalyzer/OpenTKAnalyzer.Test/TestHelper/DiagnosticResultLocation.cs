using System;

namespace OpenTKAnalyzer.Test.TestHelper
{
	/// <summary>
	/// Location where the diagnostic appears, as determined by path, line number, and column number.
	/// </summary>
	public struct DiagnosticResultLocation
	{
		public string Path { get; }
		public int Line { get; }
		public int Column { get; }

		public DiagnosticResultLocation(string path, int line, int column)
		{
			if (line < -1)
			{
				throw new ArgumentOutOfRangeException(nameof(line), nameof(line) + " must be >= -1");
			}
			if (column < -1)
			{
				throw new ArgumentOutOfRangeException(nameof(column), nameof(column) + " must be >= -1");
			}

			Path = path;
			Line = line;
			Column = column;
		}
	}
}
