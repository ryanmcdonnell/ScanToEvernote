using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Thrift.Transport;
using Thrift.Protocol;
using Evernote.EDAM.UserStore;
using Evernote.EDAM.NoteStore;
using Evernote.EDAM.Type;
using System.Security.Cryptography;
using Evernote.EDAM.Error;
using System.Diagnostics;

namespace ScanToEvernote
{
	public class EvernoteClient
	{
		private string developerToken;
		
		private const string evernoteHost = "www.evernote.com";
		private UserStore.Client userStore;

		public EvernoteClient()
		{
			Uri userStoreUrl = new Uri(String.Format("https://{0}/edam/user", evernoteHost));
			TTransport userStoreTransport = new THttpClient(userStoreUrl);
			TProtocol userStoreProtocol = new TBinaryProtocol(userStoreTransport);
			userStore = new UserStore.Client(userStoreProtocol);

			developerToken = Properties.Settings.Default.EvernoteDeveloperToken;
		}

		public bool CheckApiVersion()
		{
			return userStore.checkVersion("ScanToEvernote",
			   Evernote.EDAM.UserStore.Constants.EDAM_VERSION_MAJOR,
			   Evernote.EDAM.UserStore.Constants.EDAM_VERSION_MINOR);
		}

		private NoteStore.Client GetNoteStore()
		{
			String noteStoreUrl = userStore.getNoteStoreUrl(developerToken);
			TTransport noteStoreTransport = new THttpClient(new Uri(noteStoreUrl));
			TProtocol noteStoreProtocol = new TBinaryProtocol(noteStoreTransport);
			return new NoteStore.Client(noteStoreProtocol);
		}

		public Note NewNote(byte[] attachment, string attachmentMimeType)
		{
			Note note = new Note();
			note.Title = "Scanned document";

			byte[] hash = new MD5CryptoServiceProvider().ComputeHash(attachment);
			Data data = new Data { Size = attachment.Length, BodyHash = hash, Body = attachment };
			Resource resource = new Resource { Mime = attachmentMimeType, Data = data };

			note.Resources = new List<Resource>();
			note.Resources.Add(resource);

			string hashHex = BitConverter.ToString(hash).Replace("-", "").ToLower();
			note.Content = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>" +
				"<!DOCTYPE en-note SYSTEM \"http://xml.evernote.com/pub/enml2.dtd\">" +
				"<en-note>" +
				"<en-media type=\"" + attachmentMimeType + "\" hash=\"" + hashHex + "\"/>" +
				"</en-note>";

			return GetNoteStore().createNote(developerToken, note);
		}
	}
}
