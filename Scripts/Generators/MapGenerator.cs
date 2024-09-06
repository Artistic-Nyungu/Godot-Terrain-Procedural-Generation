using Godot;
using Godot.Collections;
using System;
using System.Linq;

[Tool]
public partial class MapGenerator : Node
{
    [Export]
	public Mesh Mesh
	{
		get{return MeshInstance.Mesh;}
		set{MeshInstance.Mesh = value;}
	}

	[ExportGroup("Graphical Properties")]
	[Export]
	public Material Material
	{
		get{return MeshInstance.MaterialOverride;}
		set{MeshInstance.MaterialOverride = value;}
	}
	[Export(PropertyHint.Range, "0,1,0.05")]
	public float BlendArea { get; set; } = .25f;

	[Export(PropertyHint.ColorNoAlpha)]
	public Color[] Colors {get; set;} = {Color.FromHtml("#e39d5f"), Color.FromHtml("#083809"), Color.FromHtml("#42f545")};

	public MeshInstance3D MeshInstance { get; private set; } = new MeshInstance3D();
	[ExportGroup("Map Properties")]
	[Export]
	public int Depth { get; set; } = 50;
	[Export]
	public int Width { get; set; } = 75;
	[Export]
	public int Resolution { get; set; } = 6;
	[Export]
	public float Amplitude{ get; set; } = 30.5f;
	[Export(PropertyHint.Range, "0,1,0.05")]
	public float Frequency{ get; set; } = 0.5f;
	[Export]
	bool IsUpToDate{ get; set; } = true;
	[ExportSubgroup("Noise Properties")]
	[Export]
	public int Layers { get; set; } = 4;
	[Export(PropertyHint.Range, "0,1,0.05")]
	public float Persistence { get; set; } = 0.5f;
	[Export]
	public float Lacunarity { get; set; } = 2f;
	[ExportGroup("Transform Properties")]
	[Export]
	public Vector3 Position
	{
		get{return MeshInstance.Position;}
		set{MeshInstance.Position = value;}
	}

	public override void _Ready()
	{
		GD.Print("Map Generator Ready");

		if(!Engine.IsEditorHint())
			IsUpToDate = false;

		/*
		PlaneMesh planeMesh = new PlaneMesh
		{
			Size = new Vector2(Width, Depth),
			SubdivideWidth = Width * Resolution,
			SubdivideDepth = Depth * Resolution
		};

		SurfaceTool surfaceTool = new SurfaceTool();
		surfaceTool.CreateFrom(planeMesh, 0);

		MeshInstance.Mesh = surfaceTool.Commit();
		MeshInstance.CastShadow = GeometryInstance3D.ShadowCastingSetting.On;
		//MeshInstance.ScriptChanged += delegate{MeshInstance.Show();};	
		*/
		
		UpdateMesh();

		AddChild(MeshInstance);
	}

    public override void _Process(double delta)
    {
		if(!IsUpToDate)
			UpdateMesh();
    }

    public override void _ValidateProperty(Dictionary property)
    {
        base._ValidateProperty(property);
		IsUpToDate = false;
    }

    private void UpdateMesh()
	{
		GD.Print("Updating Map Mesh");
		
        SurfaceTool surfaceTool = new SurfaceTool();

		surfaceTool.Begin(Mesh.PrimitiveType.Triangles);

		Noise.Initialize(Layers, 1f/(float)Resolution, Width, Depth, Frequency, Amplitude, Lacunarity, Persistence);

		float max_height = 0;
		for(int x=0; x<Resolution;x++)
		{
			for(int z=0; z<Resolution;z++)
			{
				float actualX = x*(Width/(float)(Resolution-1));
				float actualZ = z*(Depth/(float)(Resolution-1));
				float Height = Noise.GetNoise2D(actualX, actualZ);

				if(Height > max_height)
					max_height = Height;

				Vector3 Vertex = new Vector3(actualX-Width/2f, Height,actualZ-Depth/2f);

				surfaceTool.SetNormal(Vector3.Up);
				surfaceTool.SetUV(new Vector2(x/(float)Resolution, z/(float)Resolution));
				surfaceTool.AddVertex(Vertex);
			}
		}

		for (int x = 0; x < Resolution - 1; x++)
        {
            for (int z = 0; z < Resolution - 1; z++)
            {
                int i = z * Resolution + x;

                // First triangle
                surfaceTool.AddIndex(i);
                surfaceTool.AddIndex(i + Resolution);
                surfaceTool.AddIndex(i + 1);

                // Second triangle
                surfaceTool.AddIndex(i + 1);
                surfaceTool.AddIndex(i + Resolution);
                surfaceTool.AddIndex(i + Resolution + 1);
            }
        }

		//surfaceTool.GenerateNormals();
		//surfaceTool.GenerateTangents();

		Mesh newMesh = surfaceTool.Commit();
		//newMesh.SurfaceSetMaterial(0, Mesh.SurfaceGetMaterial(0));
		Mesh = newMesh;

		MeshTexture texture = new MeshTexture();
		texture.Mesh = Mesh;
	
		if(!Engine.IsEditorHint())
		{
			MeshInstance.SetInstanceShaderParameter("max_height", max_height);
			MeshInstance.SetInstanceShaderParameter("width", Width);
			MeshInstance.SetInstanceShaderParameter("depth", Depth);
			MeshInstance.SetInstanceShaderParameter("number_of_colors", Colors.Length);
			MeshInstance.SetInstanceShaderParameter("colors", Colors.Select(color => new Vector3(color.R, color.G, color.B)).ToArray());
			MeshInstance.SetInstanceShaderParameter("blend_area", BlendArea);
			MeshInstance.SetInstanceShaderParameter("normal_map", texture); 
		}
		else
		{
			if(Material is ShaderMaterial shaderMaterial)
			{
				shaderMaterial.SetShaderParameter("max_height", max_height);
				shaderMaterial.SetShaderParameter("width", Width);
				shaderMaterial.SetShaderParameter("depth", Depth);
				shaderMaterial.SetShaderParameter("number_of_colors", Colors.Length);
				shaderMaterial.SetShaderParameter("colors", Colors.Select(color => new Vector3(color.R, color.G, color.B)).ToArray());
				shaderMaterial.SetShaderParameter("blend_area", BlendArea);
				shaderMaterial.SetShaderParameter("normal_map", texture); 
			}
		}

		MeshInstance.CreateTrimeshCollision();
		MeshInstance.CastShadow = GeometryInstance3D.ShadowCastingSetting.On;
		MeshInstance.Show();

		IsUpToDate = true;
	}
}
