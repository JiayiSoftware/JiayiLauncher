using System;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using static JiayiLauncher.Utils.Imports;

namespace JiayiLauncher.Features.Versions;

public static class RequestFactory
{
	// code from MCMrARM/mc-w10-version-launcher, licensed under GPLv3; what a coincidence
	
	private static readonly string _downloadUrl = "https://fe3.delivery.mp.microsoft.com/ClientWebService/client.asmx/secured";
	
	private static readonly XNamespace _soap = "http://www.w3.org/2003/05/soap-envelope";
	private static readonly XNamespace _addressing = "http://www.w3.org/2005/08/addressing";
	private static readonly XNamespace _secext = "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd";
	private static readonly XNamespace _secutil = "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd";
	private static readonly XNamespace _updateService = "http://www.microsoft.com/SoftwareDistribution/Server/ClientWebService";
	private static readonly XNamespace _updateAuth = "http://schemas.microsoft.com/msus/2014/10/WindowsUpdateAuthorization";

	private static string GetDownloadUrl() => _downloadUrl;

	private static string GetUpdateToken()
	{
		try
		{
			var result = GetWUToken(out var token);
			
			if (result is >= WU_ERRORS_START and <= WU_ERRORS_END)
			{
				throw new Exception("Windows update error: " + result);
			}
			
			if (result != 0) Marshal.ThrowExceptionForHR(result);
			
			return token;
		}
		catch (SEHException e)
		{
			Marshal.ThrowExceptionForHR(e.HResult);
		}
		
		return string.Empty;
	}

	private static XElement BuildUpdateTickets()
	{
		var tickets = new XElement(_updateAuth + "WindowsUpdateTicketsToken",
			new XAttribute(_secutil + "id", "ClientMSA"),
			new XAttribute(XNamespace.Xmlns + "wsu", _secutil),
			new XAttribute(XNamespace.Xmlns + "wuws", _updateAuth));
		tickets.Add(new XElement("TicketType",
			new XAttribute("Name", "MSA"),
			new XAttribute("Version", "1.0"),
			new XAttribute("Policy", "MBI_SSL"),
			new XElement("User", GetUpdateToken())));
		tickets.Add(new XElement("TicketType", "",
			new XAttribute("Name", "AAD"),
			new XAttribute("Version", "1.0"),
			new XAttribute("Policy", "MBI_SSL")));
		return tickets;
	}
	
	private static XElement BuildHeader(string url, string methodName) {
		var now = DateTime.UtcNow;
		var header = new XElement(_soap + "Header",
			new XElement(_addressing + "Action", "http://www.microsoft.com/SoftwareDistribution/Server/ClientWebService/" + methodName,
				new XAttribute(_soap + "mustUnderstand", 1)),
			new XElement(_addressing + "MessageID", "urn:uuid:5754a03d-d8d5-489f-b24d-efc31b3fd32d"),
			new XElement(_addressing + "To", url,
				new XAttribute(_soap + "mustUnderstand", 1)),
			new XElement(_secext + "Security",
				new XAttribute(_soap + "mustUnderstand", 1),
				new XAttribute(XNamespace.Xmlns + "o", _secext),
				new XElement(_secutil + "Timestamp",
					new XElement(_secutil + "Created", now.ToString("o")),
					new XElement(_secutil + "Expires", now.AddMinutes(5).ToString("o"))),
				BuildUpdateTickets()));
		return header;
	}
	
	public static async Task<string> GetDownloadUrl(string updateId)
	{
		var envelope = new XElement(_soap + "Envelope");
		envelope.Add(new XAttribute(XNamespace.Xmlns + "a", _addressing));
		envelope.Add(new XAttribute(XNamespace.Xmlns + "s", _soap));
		envelope.Add(BuildHeader(GetDownloadUrl(), "GetExtendedUpdateInfo2"));
		envelope.Add(new XElement(_soap + "Body",
			new XElement(_updateService + "GetExtendedUpdateInfo2",
				new XElement(_updateService + "updateIDs",
					new XElement(_updateService + "UpdateIdentity",
						new XElement(_updateService + "UpdateID", updateId),
						new XElement(_updateService + "RevisionNumber", 1))), // revision number was an argument, but it's always 1
				new XElement(_updateService + "infoTypes",
					new XElement(_updateService + "XmlUpdateFragmentType", "FileUrl")),
				new XElement(_updateService + "deviceAttributes", "E:BranchReadinessLevel=CBB&DchuNvidiaGrfxExists=1&ProcessorIdentifier=Intel64%20Family%206%20Model%2063%20Stepping%202&CurrentBranch=rs4_release&DataVer_RS5=1942&FlightRing=Retail&AttrDataVer=57&InstallLanguage=en-US&DchuAmdGrfxExists=1&OSUILocale=en-US&InstallationType=Client&FlightingBranchName=&Version_RS5=10&UpgEx_RS5=Green&GStatus_RS5=2&OSSkuId=48&App=WU&InstallDate=1529700913&ProcessorManufacturer=GenuineIntel&AppVer=10.0.17134.471&OSArchitecture=AMD64&UpdateManagementGroup=2&IsDeviceRetailDemo=0&HidOverGattReg=C%3A%5CWINDOWS%5CSystem32%5CDriverStore%5CFileRepository%5Chidbthle.inf_amd64_467f181075371c89%5CMicrosoft.Bluetooth.Profiles.HidOverGatt.dll&IsFlightingEnabled=0&DchuIntelGrfxExists=1&TelemetryLevel=1&DefaultUserRegion=244&DeferFeatureUpdatePeriodInDays=365&Bios=Unknown&WuClientVer=10.0.17134.471&PausedFeatureStatus=1&Steam=URL%3Asteam%20protocol&Free=8to16&OSVersion=10.0.17134.472&DeviceFamily=Windows.Desktop"))));
		var doc = new XDocument(envelope);
		
		using var client = new HttpClient();
		using var request = new HttpRequestMessage(HttpMethod.Post, _downloadUrl);
		
		request.Content = new StringContent(doc.ToString(SaveOptions.DisableFormatting), Encoding.UTF8,
			"application/soap+xml");
		
		using var response = await client.SendAsync(request);
		var responseString = await response.Content.ReadAsStringAsync();
		var responseDoc = XDocument.Parse(responseString);
		
		var manager = new XmlNamespaceManager(new NameTable());
		manager.AddNamespace("s", "http://www.w3.org/2003/05/soap-envelope");
		manager.AddNamespace("wu", "http://www.microsoft.com/SoftwareDistribution/Server/ClientWebService");
		
		var result = responseDoc.XPathSelectElement(
			"/s:Envelope/s:Body/wu:GetExtendedUpdateInfo2Response/wu:GetExtendedUpdateInfo2Result",
			manager);
		
		if (result is null) return string.Empty;

		var elements = 
			result.XPathSelectElements("wu:FileLocations/wu:FileLocation/wu:Url", manager)
			.Select(e => e.Value)
			.Where(url => url.StartsWith("http://tlu.dl.delivery.mp.microsoft.com/"));
		
		return elements.FirstOrDefault() ?? string.Empty;
	}
}