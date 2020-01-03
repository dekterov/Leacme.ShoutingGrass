// Copyright (c) 2017 Leacme (http://leac.me). View LICENSE.md for more information.
using Godot;
using System;
using System.Linq;
using System.Text.RegularExpressions;

public class Menu : Node2D {

	private VBoxContainer root_holder;

	public override void _Ready() {
		root_holder = GetNode<VBoxContainer>("root_holder");

		GetViewport().Connect("size_changed", this, nameof(OnViewportResize));
		root_holder.GetNode<Button>("start_button").Connect("pressed", this, nameof(OnPressed_start));
		root_holder.GetNode<Button>("options_button").Connect("pressed", this, nameof(OnPressed_options));
		root_holder.GetNode<Button>("about_button").Connect("pressed", this, nameof(OnPressed_about));
		GetNode("about_popup").GetNode("about_holder").GetNode<Button>("close_button").Connect("pressed", this, nameof(OnPressed_about_close));
		GetNode("about_popup").GetNode("about_holder").GetNode<RichTextLabel>("c_label").Connect("meta_clicked", this, nameof(OnLabel_url_clicked));
		GetNode("options_popup").GetNode("options_holder").GetNode<Button>("close_button").Connect("pressed", this, nameof(OnPressed_options_close));

		root_holder.RectMinSize = GetViewportRect().Size;
		ThemeDefaultHolder(root_holder);
		root_holder.GetNode<Label>("title").AddColorOverride("font_color", new Color(1, 1, 1));
		ThemeButtons(GetNode<VBoxContainer>("root_holder"));

	}

	private void OnLabel_url_clicked(string url) {
		var err = OS.ShellOpen(url);
	}

	private void ThemeDefaultHolder(BoxContainer holder) {
		holder.Alignment = BoxContainer.AlignMode.Center;
		holder.AddConstantOverride("separation", 20);

		holder.GetNode<TextureRect>("logo").Texture = GD.Load<Texture>("res://assets/default/icon.png");
		holder.GetNode<TextureRect>("logo").SizeFlagsHorizontal = (int)Control.SizeFlags.ShrinkCenter;

		var title = holder.GetNode<Label>("title");
		title.Align = Label.AlignEnum.Center;
		title.Text = ProjectSettings.GetSetting("application/config/name").ToString().Replace("Leacme ", "Leacme\n");
		title.Autowrap = true;
		title.AddFontOverride("font", new DynamicFont() { FontData = GD.Load<DynamicFontData>("res://assets/default/Tuffy_Bold.ttf"), Size = 70 });

	}

	private void ThemeButtons(Control buttonHolder) {
		foreach (var child in buttonHolder.GetChildren()) {
			if (child is Button bt) {
				bt.SizeFlagsHorizontal = (int)Control.SizeFlags.ShrinkCenter;
				bt.RectMinSize = new Vector2(root_holder.RectMinSize.x * 0.7f, 40);
				bt.AddFontOverride("font", new DynamicFont() { FontData = GD.Load<DynamicFontData>("res://assets/default/Tuffy_Bold.ttf"), Size = 40 });
			}
		}
	}

	private void OnPressed_start() {
		GetTree().ChangeScene("res://scenes/Main.tscn");
	}

	private void OnPressed_options() {
		var popup = GetNode<PopupPanel>("options_popup");
		popup.PopupExclusive = true;
		popup.PopupCenteredRatio(0.85f);
		var options_holder = popup.GetNode<VBoxContainer>("options_holder");
		options_holder.Alignment = BoxContainer.AlignMode.Center;
		options_holder.AddConstantOverride("separation", 20);
		var options_scroll = options_holder.GetNode<ScrollContainer>("options_scroll");
		options_scroll.RectMinSize = new Vector2(options_scroll.RectMinSize.x, (options_holder.RectSize.y - options_holder.GetNode<Button>("close_button").RectSize.y) * 0.9f);
		var options_scroll_holder = options_scroll.GetNode<VBoxContainer>("options_scroll_holder");
		options_scroll_holder.GetChildren().Cast<Control>().ToList().ForEach(z => z.QueueFree());
		AddOptions(options_scroll_holder);

		ThemeButtons(options_holder);
	}

	private void AddOptions(VBoxContainer options_scroll_holder) {

		var bcBt = new CheckButton();
		bcBt.Text = "Show Border:";
		bcBt.Connect("toggled", this, nameof(OnBorderEnabledToggled));
		bcBt.Pressed = Lib.Node.BoderEnabled;
		options_scroll_holder.AddChild(bcBt);

		options_scroll_holder.AddChild(new Label() { Text = "Border Color:" });
		var bcp = new ColorPickerButton();
		bcp.Connect("color_changed", this, nameof(OnNewBorderColor));
		bcp.Color = new Color(Lib.Node.BorderColorHtmlCode);
		options_scroll_holder.AddChild(bcp);

		options_scroll_holder.AddChild(new Label() { Text = "Background Color:" });
		var bgcp = new ColorPickerButton();
		bgcp.Connect("color_changed", this, nameof(OnNewBackgroundColor));
		bgcp.Color = new Color(Lib.Node.BackgroundColorHtmlCode);
		options_scroll_holder.AddChild(bgcp);

		var sdBt = new CheckButton();
		sdBt.Text = "Sound Enabled:";
		sdBt.Connect("toggled", this, nameof(OnSoundEnabledToggled));
		sdBt.Pressed = Lib.Node.SoundEnabled;
		options_scroll_holder.AddChild(sdBt);

		var vgBt = new CheckButton();
		vgBt.Text = "Show Vignette:";
		vgBt.Connect("toggled", this, nameof(OnVignetteEnabledToggled));
		vgBt.Pressed = Lib.Node.VignetteEnabled;
		options_scroll_holder.AddChild(vgBt);

	}

	private void OnNewBackgroundColor(Color color) {
		Lib.Node.BackgroundColorHtmlCode = color.ToHtml();
	}

	private void OnBorderEnabledToggled(bool isOn) {
		Lib.Node.BoderEnabled = isOn;
		Update();
	}

	private void OnNewBorderColor(Color color) {
		Lib.Node.BorderColorHtmlCode = color.ToHtml();
		Update();
	}

	private void OnSoundEnabledToggled(bool isOn) {
		Lib.Node.SoundEnabled = isOn;
	}

	private void OnVignetteEnabledToggled(bool isOn) {
		Lib.Node.VignetteEnabled = isOn;
	}

	private void OnPressed_options_close() {
		GetNode<PopupPanel>("options_popup").Hide();
	}

	private void OnPressed_about() {
		var popup = GetNode<PopupPanel>("about_popup");
		popup.PopupExclusive = true;
		popup.PopupCenteredRatio(0.85f);

		ThemeDefaultHolder(popup.GetNode<VBoxContainer>("about_holder"));
		popup.GetNode<VBoxContainer>("about_holder").GetNode<Label>("title").AddFontOverride("font", new DynamicFont() { FontData = GD.Load<DynamicFontData>("res://assets/default/Tuffy_Bold.ttf"), Size = 40 });
		var logo = popup.GetNode<VBoxContainer>("about_holder").GetNode<TextureRect>("logo");
		logo.Expand = true;
		logo.RectMinSize = logo.Texture.GetSize() * 0.7f;

		ThemeButtons(popup.GetNode<VBoxContainer>("about_holder"));

		var c_label = popup.GetNode<VBoxContainer>("about_holder").GetNode<RichTextLabel>("c_label");
		c_label.BbcodeEnabled = true;
		c_label.RectMinSize = new Vector2(c_label.RectMinSize.x, 150);
		c_label.BbcodeText = "[center]Copyright (c) 2017 Leacme\n([color=#996600][url=http://leac.me]http://leac.me[/url][/color])\n\n[/center]";

		c_label.BbcodeText += "[center][u]USAGE\n[/u][/center]";
		using (var instrFil = new Godot.File()) {
			instrFil.Open("res://README.md", File.ModeFlags.Read);
			c_label.BbcodeText += "This application features the ability to " + instrFil.GetAsText().Split(new string[] { "This application features the ability to" }, StringSplitOptions.None)[1].Split("![][image_screenshot]")[0].Trim() + "\n\n";
			c_label.BbcodeText += instrFil.GetAsText().Split(new string[] { "## Application Usage" }, StringSplitOptions.None)[1].Split("## Copyright")[0].Trim() + "\n\n";
			instrFil.Close();
		}

		c_label.BbcodeText += "[center][u]LICENSES\n[/u][/center]";
		c_label.BbcodeText += ProjectSettings.GetSetting("application/config/name").ToString() + ":\n";
		using (var leacLic = new Godot.File()) {
			leacLic.Open("res://LICENSE.md", File.ModeFlags.Read);
			c_label.BbcodeText += leacLic.GetAsText();
			leacLic.Close();
		}
	}

	private void OnPressed_about_close() {
		GetNode<PopupPanel>("about_popup").Hide();
	}

	public override void _Draw() {
		Hud.DrawBorder(this);
	}

	public void OnViewportResize() {
		root_holder.RectMinSize = GetViewportRect().Size;
	}

}
