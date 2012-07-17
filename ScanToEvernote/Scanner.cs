using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WIA;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;

namespace ScanToEvernote
{
	class Scanner
	{
		#region WIA constants
		public const int WIA_DPS_DOCUMENT_HANDLING_CAPABILITIES = 3086;
		public const int WIA_DPS_DOCUMENT_HANDLING_STATUS = 3087;
		public const int WIA_DPS_DOCUMENT_HANDLING_SELECT = 3088;

		public const string WIA_FORMAT_JPEG = "{B96B3CAE-0728-11D3-9D7B-0000F81EF32E}";

		public const int FEED_READY = 0x00000001;

		public const int BASE_VAL_WIA_ERROR = 0x80210000;
		public const int WIA_ERROR_PAPER_EMPTY = BASE_VAL_WIA_ERROR + 3;
		#endregion

		private string _deviceId;
		private DeviceInfo _deviceInfo;
		private Device _device;

		public Scanner()
		{
			_deviceId = GetDefaultDeviceID();
			_deviceInfo = FindDevice(_deviceId);
			_device = _deviceInfo.Connect();
		}

		private DeviceInfo FindDevice(string deviceId)
		{
			DeviceManager manager = new DeviceManager();
			foreach (DeviceInfo info in manager.DeviceInfos)
				if (info.DeviceID == deviceId)
					return info;

			return null;
		}

		private string GetDefaultDeviceID()
		{
			string deviceId = Properties.Settings.Default.ScannerDeviceID;
			if (String.IsNullOrEmpty(deviceId))
			{
				// Select a scanner
				CommonDialog wiaDiag = new CommonDialog();
				Device d = wiaDiag.ShowSelectDevice(WiaDeviceType.ScannerDeviceType, true, false);
				if (d != null)
				{
					deviceId = d.DeviceID;
					Properties.Settings.Default.ScannerDeviceID = deviceId;
					Properties.Settings.Default.Save();
				}
			}

			return deviceId;
		}

		public List<Image> ScanPages(int dpi = 150, double width = 8.5, double height = 11)
		{
			Item item = _device.Items[1];

			// configure item
			SetDeviceItemProperty(ref item, 6146, 2); // greyscale
			SetDeviceItemProperty(ref item, 6147, dpi); // 150 dpi
			SetDeviceItemProperty(ref item, 6148, dpi); // 150 dpi
			SetDeviceItemProperty(ref item, 6151, (int)(dpi * width)); // scan width
			SetDeviceItemProperty(ref item, 6152, (int)(dpi * height)); // scan height
			SetDeviceItemProperty(ref item, 4104, 8); // bit depth

			int deviceHandling = true ? 1 : 2; // 1 for ADF, 2 for flatbed
			SetDeviceProperty(ref _device, 3088, deviceHandling);

			// TODO: Detect if the ADF is loaded, if not use the flatbed

			List<Image> images = new List<Image>();

			int handlingStatus = GetDeviceProperty(ref _device, WIA_DPS_DOCUMENT_HANDLING_STATUS);
			if (handlingStatus == FEED_READY)
			{
				do
				{
					ImageFile wiaImage = null;
					try
					{
						wiaImage = item.Transfer(WIA_FORMAT_JPEG);
					}
					catch (COMException ex)
					{
						if (ex.ErrorCode == WIA_ERROR_PAPER_EMPTY)
							break;
						else
							throw;
					}

					if (wiaImage != null)
					{
						Image image = ConvertToImage(wiaImage);
						images.Add(image);
					}
				}
				while (true);
			}

			return images;
		}

		private static Image ConvertToImage(ImageFile wiaImage)
		{
			byte[] imageBytes = (byte[])wiaImage.FileData.get_BinaryData();
			MemoryStream ms = new MemoryStream(imageBytes);
			Image image = Image.FromStream(ms);
			return image;
		}

		#region Get/set device properties

		private void SetDeviceProperty(ref Device device, int propertyID, int propertyValue)
		{
			foreach (Property p in device.Properties)
			{
				if (p.PropertyID == propertyID)
				{
					object value = propertyValue;
					p.set_Value(ref value);
					break;
				}
			}
		}

		private int GetDeviceProperty(ref Device device, int propertyID)
		{
			int ret = -1;

			foreach (Property p in device.Properties)
			{
				if (p.PropertyID == propertyID)
				{
					ret = (int)p.get_Value();
					break;
				}
			}

			return ret;
		}

		private void SetDeviceItemProperty(ref Item item, int propertyID, int propertyValue)
		{
			foreach (Property p in item.Properties)
			{
				if (p.PropertyID == propertyID)
				{
					object value = propertyValue;
					p.set_Value(ref value);
					break;
				}
			}
		}

		private int GetDeviceItemProperty(ref Item item, int propertyID)
		{
			int ret = -1;

			foreach (Property p in item.Properties)
			{
				if (p.PropertyID == propertyID)
				{
					ret = (int)p.get_Value();
					break;
				}
			}

			return ret;
		}

		#endregion
	}
}
