using UnityEngine;
using System.Collections;

namespace UFaceAnim
{
	/// <summary>
	/// This structure serves as a library or lookup table for storing and accessing
	/// various pre-built blend states not covered by emotion or speech based blending.
	/// Examples for this would be grimaces or a particularly dumb look on the face of
	/// a character that they might display in cutscenes or whenever some key event
	/// takes place. The entries can simply be accessed via array lookup, and a state's
	/// purpose identified by the entry's handle key. Since it's a plain old array,
	/// you may as well just use the index of an entry as unique ID number.
	/// </summary>
	[System.Serializable]
	public struct FABlendLibrary
	{
		#region Fields

		public FABlendStateHandle[] entries;

		#endregion
		#region Properties

		public static FABlendLibrary Empty
		{
			get
			{
				FABlendLibrary bl = new FABlendLibrary();

				bl.entries = null;

				return bl;
			}
		}

		#endregion
		#region Methods

		public bool getState(int index, ref FABlendStateHandle outState)
		{
			if(entries == null || index >= entries.Length) return false;

			outState = entries[index];
			return true;
		}
		public bool getState(string key, ref FABlendStateHandle outState)
		{
			if(entries == null || entries.Length == 0) return false;

			for(int i = 0; i < entries.Length; ++i)
			{
				if(string.Compare(entries[i].key, key, true) == 0)
				{
					outState = entries[i];
					return true;
				}
			}
			return false;
		}
		public bool setState(string key, FABlendState state, FABlendOverwrite overwrite = FABlendOverwrite.Emotion)
		{
			if(string.IsNullOrEmpty(key)) return false;

			FABlendStateHandle newEntry;
			newEntry.key=key;
			newEntry.state = state;
			newEntry.overwrite = overwrite;

			// If there were no entries previously, create them from this one new entry:
			if(entries == null)
			{
				entries = new FABlendStateHandle[1] { newEntry };
				return true;
			}

			// Alternatively, find an existing entry with the same key and update data:
			for(int i = 0; i < entries.Length; ++i)
			{
				FABlendStateHandle entry = entries[i];
				if(string.Compare(key, entry.key) == 0)
				{
					entry.state = state;
					entry.overwrite = overwrite;

					return true;
				}
			}

			// Otherwise create new entry and copy all previous entries into a new, resized array:
			int newLength = entries.Length + 1;
			FABlendStateHandle[] newEntries = new FABlendStateHandle[newLength];
			for(int i = 0; i < entries.Length; ++i)
			{
				newEntries[i] = entries[i];
			}
			newEntries[newLength - 1] = newEntry;

			entries = newEntries;
			return true;
		}

		public int getStateIndex(string key)
		{
			if(entries == null) return -1;

			for(int i = 0; i < entries.Length; ++i)
			{
				if(string.Compare(entries[i].key, key, true) == 0)
					return i;
			}
			return -1;
		}
		public string getStateKey(int index)
		{
			if(entries == null || index < 0 || index >= entries.Length) return null;

			return entries[index].key;
		}

		#endregion
	}
}
