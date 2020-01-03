// Copyright (c) 2017 Leacme (http://leac.me). View LICENSE.md for more information.
using Godot;
using System;

public class Main : Spatial {

	public AudioStreamPlayer Audio { get; } = new AudioStreamPlayer();

	private void InitSound() {
		if (!Lib.Node.SoundEnabled) {
			AudioServer.SetBusMute(AudioServer.GetBusIndex("Master"), true);
		}
	}

	public override void _Notification(int what) {
		if (what is MainLoop.NotificationWmGoBackRequest) {
			GetTree().ChangeScene("res://scenes/Menu.tscn");
		}
	}

	public override void _Ready() {
		var env = GetNode<WorldEnvironment>("sky").Environment;
		env.BackgroundMode = Godot.Environment.BGMode.Sky;
		env.BackgroundSky = new PanoramaSky() { Panorama = ((Texture)GD.Load("res://assets/park.hdr")) };
		env.BackgroundSkyRotationDegrees = new Vector3(0, 140, 0);

		GD.Load<AudioStream>("res://assets/field.ogg").Play(Audio);

		var sun1 = new DirectionalLight() {
			LightEnergy = 0.4f
		};
		var sun2 = new DirectionalLight() {
			LightEnergy = 0.4f,
			LightIndirectEnergy = 0
		};
		sun2.RotationDegrees = new Vector3(-62, -145, 0);
		AddChild(sun1);
		AddChild(sun2);

		InitSound();
		AddChild(Audio);

		var gMat = new ShaderMaterial() {
			Shader = new Shader() {
				Code = @"
					shader_type spatial;

					render_mode blend_mix,depth_draw_opaque,cull_disabled,diffuse_burley,specular_schlick_ggx;

					uniform vec4 albedo : hint_color;
					uniform sampler2D texture_albedo : hint_albedo;

					uniform float freq = 2f;
					uniform float range = 2f;
					uniform float pow = 0.1;

					void vertex() {
						if (VERTEX.y > 0.0) {
							vec3 vertPos = (vec4(VERTEX, 1.0) * WORLD_MATRIX).xyz;
							float disp = sin(vertPos.x * range + TIME * freq) * pow;
							VERTEX.x += disp;
							VERTEX.z += disp;
						}
					}

					void fragment() {
						vec2 base_uv = UV;
						vec4 albedo_tex = texture(texture_albedo,base_uv);
						ALBEDO = albedo.rgb * albedo_tex.rgb;
						METALLIC = 0f;
						SPECULAR = 0.5;
						ROUGHNESS = 1f;
						ALPHA = albedo.a * albedo_tex.a;
						ALPHA_SCISSOR=0.6;
					}"
			}
		};
		gMat.SetShaderParam("albedo", new Color("ff999900"));
		gMat.SetShaderParam("texture_albedo", GD.Load<Texture>("res://assets/grass5.png"));

		var bMat = new ShaderMaterial() {
			Shader = new Shader() {
				Code = @"
					shader_type spatial;

					render_mode blend_mix,depth_draw_opaque,cull_disabled,diffuse_burley,specular_schlick_ggx;

					uniform vec4 albedo : hint_color;
					uniform sampler2D texture_albedo : hint_albedo;

					uniform float freq = 16f;
					uniform float range = 0.7f;

					void vertex() {
						VERTEX.y += sin(TIME * freq) * (1.0 - sin(3.14 * UV.x)) * range;
					}

					void fragment() {
						vec2 base_uv = UV;
						vec4 albedo_tex = texture(texture_albedo,base_uv);
						ALBEDO = albedo.rgb * albedo_tex.rgb;
						METALLIC = 0f;
						SPECULAR = 0.5;
						ROUGHNESS = 1f;
						ALPHA = albedo.a * albedo_tex.a;
						ALPHA_SCISSOR=0.6;
					}"
			}
		};
		bMat.SetShaderParam("albedo", new Color("ffffffff"));
		bMat.SetShaderParam("texture_albedo", GD.Load<Texture>("res://assets/butterfly.png"));

		float height = 0;
		for (int i = -3; i < 30; i++) {
			for (int j = -3; j < 3; j++) {

				var gInt = (Spatial)GD.Load<PackedScene>("res://scenes/Grass.tscn").Instance();
				var gmesh = gInt.GetNode<MeshInstance>("Grass");
				gmesh.MaterialOverride = gMat;
				var tempScale = gInt.Scale;
				tempScale.y *= 0.8f;
				tempScale *= (float)GD.RandRange(0.3f, 0.7f);
				gInt.Scale = tempScale;
				gInt.RotateY(Mathf.Deg2Rad((float)GD.RandRange(-180, 180)));
				if ((i % 2) == 0) {
					gInt.Translation = new Vector3(i, height -= 0.04f, j);
				} else {
					gInt.Translation = new Vector3(i, height, j + 0.5f);
				}

				void AddAdditionalGrass(Vector3 pos) {
					var additionalGrass2 = (Spatial)gInt.Duplicate();
					var gmat2 = (ShaderMaterial)gMat.Duplicate();
					gmat2.SetShaderParam("albedo", new Color("ff999900"));
					additionalGrass2.GetNode<MeshInstance>("Grass").MaterialOverride = gmat2;
					additionalGrass2.Translate(pos);
					AddChild(additionalGrass2);
				}

				if (i < 5) {
					AddAdditionalGrass(new Vector3(1.5f, 0, 0));
					AddAdditionalGrass(new Vector3(-1.5f, 0, 0));
				}

				if (i > 10) {
					AddAdditionalGrass(new Vector3(0, 0, 11f));
					AddAdditionalGrass(new Vector3(0, 0, -11f));
				}

				AddChild(gInt);
			}
		}

		var bMesh = new PlaneMesh() {
			Size = new Vector2(1, 1),
			SubdivideDepth = 1,
			SubdivideWidth = 1,
			Material = bMat
		};

		var bPartR = new CPUParticles() {
			Mesh = bMesh,
			Scale = Scale * 0.3f,
			Gravity = new Vector3(0, 0, 0),
			Direction = new Vector3(0, 0, -1),
			InitialVelocity = 30,
			Amount = 30,
			Spread = 30,
			Lifetime = 20,
			InitialVelocityRandom = 0.80f,
			EmissionShape = CPUParticles.EmissionShapeEnum.Sphere,
			EmissionSphereRadius = 1,

		};

		AddChild(bPartR);

		var bPartL = (CPUParticles)bPartR.Duplicate();
		AddChild(bPartL);

		bPartR.Translate(new Vector3(15, 5, 25));
		bPartL.RotationDegrees = new Vector3(0, 180, 0);
		bPartL.Translate(new Vector3(-15, 5, 25));

	}

	public override void _Process(float delta) {

	}

}
