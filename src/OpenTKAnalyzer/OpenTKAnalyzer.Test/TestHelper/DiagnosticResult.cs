using Microsoft.CodeAnalysis;

namespace OpenTKAnalyzer.TestHelper
{
	/// <summary>
	/// Struct that stores information about a Diagnostic appearing in a source
	/// </summary>
	public struct DiagnosticResult
	{
		public DiagnosticResultLocation Location
		{
			get
			{
				return location.HasValue ? location.Value : (location = new DiagnosticResultLocation(Path, Line, Column)).Value;
			}
			set
			{
				location = value;
			}
		}
		public DiagnosticResultLocation? location;

		public DiagnosticSeverity Severity { get; set; }

		public string Id { get; set; }

		public string Message { get; set; }

		public string Path => location.HasValue ? location.Value.Path : string.Empty;

		public int Line => location.HasValue ? location.Value.Line : -1;

		public int Column => location.HasValue ? location.Value.Column : -1;
	}
}
