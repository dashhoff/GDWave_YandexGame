using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AssetKits.ParticleImage
{
    public class ParticlePool
    {
        private List<Particle> _particles;
        private ParticleImage _source;
        
        public ParticlePool(int capacity, ParticleImage source)
        {
            _particles = new List<Particle>(capacity);
            _source = source;
            
            for (int i = 0; i < capacity; i++)
            {
                _particles.Add(new Particle(source));
            }
        }
        
        public Particle Get()
        {
            if(_particles.Count > 0)
            {
                var particle = _particles[_particles.Count - 1];
                _particles.RemoveAt(_particles.Count - 1);
                return particle;
            }

            return new Particle(_source);
        }
        
        public void Release(Particle particle)
        {
            _particles.Add(particle);
        }
    }
}
