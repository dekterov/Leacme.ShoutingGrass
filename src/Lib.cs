// Copyright (c) 2017 Leacme (http://leac.me). View LICENSE.md for more information.
using System;
using System.Linq;
using Godot;
using LiteDB;

public class Lib : Node {

	private LiteDatabase db = new LiteDatabase(System.IO.Path.Combine(OS.GetUserDataDir(), typeof(Lib).Assembly.GetName().Name + " Settings.db"));
	private LiteCollection<BsonDocument> optionsCollection;

	public static Lib Node { get; private set; }

	public string BorderColorHtmlCode {
		get { return GetFromDb(nameof(BorderColorHtmlCode)); }
		set { SetToDb(nameof(BorderColorHtmlCode), value); }
	}

	public bool BoderEnabled {
		get { return GetFromDb(nameof(BoderEnabled)); }
		set { SetToDb(nameof(BoderEnabled), value); }
	}

	public string BackgroundColorHtmlCode {
		get { return GetFromDb(nameof(BackgroundColorHtmlCode)); }
		set { SetToDb(nameof(BackgroundColorHtmlCode), value); }
	}

	public bool SoundEnabled {
		get { return GetFromDb(nameof(SoundEnabled)); }
		set { SetToDb(nameof(SoundEnabled), value); }
	}

	public bool VignetteEnabled {
		get { return GetFromDb(nameof(VignetteEnabled)); }
		set { SetToDb(nameof(VignetteEnabled), value); }
	}

	private BsonValue GetFromDb(string key) {
		return optionsCollection.FindOne(z => z.ContainsKey(key))[key];
	}

	private void SetToDb(string key, BsonValue value) {
		var x = optionsCollection.FindOne(z => z.ContainsKey(key));
		x[key] = value;
		optionsCollection.Update(x);
	}

	private void InitDbEntry(string key, BsonValue value) {
		if (!optionsCollection.Exists(x => x.ContainsKey(key))) {
			optionsCollection.Insert(new BsonDocument { [key] = value });
		}
	}

	public override void _Ready() {
		Node = GetNode<Lib>("/root/Lib");
		optionsCollection = db.GetCollection(nameof(optionsCollection));
		GD.Randomize();
		InitDbEntry(nameof(BorderColorHtmlCode), "ffa4b543");
		InitDbEntry(nameof(BoderEnabled), true);
		InitDbEntry(nameof(BackgroundColorHtmlCode), "ff535353");
		InitDbEntry(nameof(SoundEnabled), true);
		InitDbEntry(nameof(VignetteEnabled), true);
		// GD.Print(String.Join(" ", optionsCollection.FindAll()));
	}

}

public static class Exts {
	public static void Play(this AudioStream sound, AudioStreamPlayer player) {
		player.Stream = sound;
		player.Play();
	}
}
