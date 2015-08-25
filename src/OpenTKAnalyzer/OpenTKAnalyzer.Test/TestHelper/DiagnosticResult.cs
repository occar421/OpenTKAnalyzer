using Microsoft.CodeAnalysis;

namespace OpenTKAnalyzer.TestHelper
{
	/// <summary>
	/// Struct that stores information about a Diagnostic appearing in a source
	/// </summary>
	public struct DiagnosticResult
	{
		private DiagnosticResultLocation[] locations;
		public DiagnosticResultLocation[] Locations
		{
			get
			{
				return (this.locations = this.locations ?? new DiagnosticResultLocation[] { });
			}
			set
			{
				this.locations = value;
			}
		}

		public DiagnosticSeverity Severity { get; set; }

		public string Id { get; set; }

		public string Message { get; set; }

		public string Path => Locations.Length > 0 ? Locations[0].Path : string.Empty;

		public int Line => Locations.Length > 0 ? Locations[0].Line : -1;

		public int Column => Locations.Length > 0 ? Locations[0].Column : -1;
	}
}
