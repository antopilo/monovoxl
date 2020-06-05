using HelloMonoGame.Chunk;
using HelloMonoGame.Entities;
using HelloMonoGame.Entities.Particles;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


public class Scene
{
    // Name of the scene
    public string Name { get; set; }

    // List of all entities
    public List<IEntity> Entities { get; set; }

    // Current camera
    public Camera CurrentCamera { get; set; }
    public static Camera DefaultCamera = new Camera(new Vector3(0, 100, 0), new Vector3(1, 100, 0), Vector3.Up);

    public static float DeltaTime = 0f;

    public static float Gravity = 0.4f;


       
    
    public Scene(string pName)
    {
        this.Name = pName;
        this.Entities = new List<IEntity>();
    }

    public void Initialize()
    {
        // Add default camera
        this.AddEntity(DefaultCamera);
        CurrentCamera = DefaultCamera;

        ChunkManager.Initialize(CurrentCamera);
        HelloMonoGame.Generation.Generator.Initialize(1337);
    }

    public void Update(float delta)
    {
        DeltaTime += delta;

        // Update each entities
        foreach (IEntity entity in this.Entities)
        {
            entity.Update(delta);
        }

        ParticleManager.Update(delta);

        if(DeltaTime > 1)
        {
            //ChunkManager.Update();
            DeltaTime = 0;
        }
    }

    public void AddEntity(IEntity entity)
    {
        this.Entities.Add(entity);
    }

    public void SetCamera(Camera cam)
    {
        this.CurrentCamera = cam;
    }
}


