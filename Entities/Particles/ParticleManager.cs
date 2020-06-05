using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelloMonoGame.Entities.Particles
{
    
    public static class ParticleManager
    {
        public static List<Particle> Particles = new List<Particle>();

        public static void SpawnCubeParticle(Vector3 position, Color color)
        {
            var p = new Particle(position, color);
            Particles.Add(p);
            Renderer.AddParticle(p);
        }

        public static void Update(float delta)
        {
            foreach (var p in Particles)
            {
                p.Update(delta);
            }
        }
    }
}
