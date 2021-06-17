using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Swarm
{
  //params
  public int NumParticles = 10;
  public FitnessFunction FitnessFunction;
  public Particle[] Particles;
  public Vector3 BestPosition;
  public float BestFitness;
  //public Vector3Int OrderIndex;

  public Swarm() : this(10, new SwarmFitnessFunction()) { }

  public Swarm(
    int numParticles,
    FitnessFunction fitnessFunction
  )
  {
    //var rand = new System.Random();
    this.Particles = new Particle[numParticles];
    for(int i = 0; i < numParticles; i++)
    {
      this.Particles[i] = new Particle(
        /*
        new Vector3(
          Random.Range(SwarmSetting.PARTICLE_LOWER_BOUNDS_POSITION.x, SwarmSetting.PARTICLE_UPPER_BOUNDS_POSITION.x),
          Random.Range(SwarmSetting.PARTICLE_LOWER_BOUNDS_POSITION.y, SwarmSetting.PARTICLE_UPPER_BOUNDS_POSITION.y),
          Random.Range(SwarmSetting.PARTICLE_LOWER_BOUNDS_POSITION.z, SwarmSetting.PARTICLE_UPPER_BOUNDS_POSITION.z)
        ),
        0.1f,
        new Vector3(
          Random.Range(SwarmSetting.PARTICLE_LOWER_BOUNDS_SPEED.x, SwarmSetting.PARTICLE_UPPER_BOUNDS_SPEED.x),
          Random.Range(SwarmSetting.PARTICLE_LOWER_BOUNDS_SPEED.y, SwarmSetting.PARTICLE_UPPER_BOUNDS_SPEED.y),
          Random.Range(SwarmSetting.PARTICLE_LOWER_BOUNDS_SPEED.z, SwarmSetting.PARTICLE_UPPER_BOUNDS_SPEED.z)
        ),
        fitnessFunction
        */
        new Vector3(
          Utils.RandomRange(SwarmSetting.PARTICLE_LOWER_BOUNDS_POSITION.x, SwarmSetting.PARTICLE_UPPER_BOUNDS_POSITION.x),
          Utils.RandomRange(SwarmSetting.PARTICLE_LOWER_BOUNDS_POSITION.y, SwarmSetting.PARTICLE_UPPER_BOUNDS_POSITION.y),
          Utils.RandomRange(SwarmSetting.PARTICLE_LOWER_BOUNDS_POSITION.z, SwarmSetting.PARTICLE_UPPER_BOUNDS_POSITION.z)
        ),
        0.1f,
        new Vector3(
          Utils.RandomRange(SwarmSetting.PARTICLE_LOWER_BOUNDS_SPEED.x, SwarmSetting.PARTICLE_UPPER_BOUNDS_SPEED.x),
          Utils.RandomRange(SwarmSetting.PARTICLE_LOWER_BOUNDS_SPEED.y, SwarmSetting.PARTICLE_UPPER_BOUNDS_SPEED.y),
          Utils.RandomRange(SwarmSetting.PARTICLE_LOWER_BOUNDS_SPEED.z, SwarmSetting.PARTICLE_UPPER_BOUNDS_SPEED.z)
        ),
        fitnessFunction
      );
    }
    this.BestPosition = new Vector3();
    this.BestFitness = float.MinValue;
    this.SetFitnessFunction(fitnessFunction);
    //this.OrderIndex = new Vector3Int();
    this.CalcParticleOrder();
  }

  public Swarm(Swarm instanse)
  {
    this.NumParticles = instanse.NumParticles;
    this.FitnessFunction = instanse.FitnessFunction;
    this.Particles = new Particle[this.NumParticles];
    for(int i = 0; i < this.Particles.Length; i++)
    {
      this.Particles[i] = new Particle(instanse.Particles[i]);
    }
    this.BestPosition = instanse.BestPosition;
    this.BestFitness = instanse.BestFitness;
}

  public void SetFitnessFunction(FitnessFunction fitnessFunction)
  {
    this.FitnessFunction = fitnessFunction;
    foreach (Particle particle in this.Particles)
    {
      particle.SetFitnessFunction(fitnessFunction);
    }
  }

  public void CalcParticleOrder()
  {
    this.CalcParticleOrder_by(0);
    this.CalcParticleOrder_by(1);
    this.CalcParticleOrder_by(2);
    /*
    this.OrderIndex.x = this.CalcParticleOrder_by(0);
    this.OrderIndex.y = this.CalcParticleOrder_by(1);
    this.OrderIndex.z = this.CalcParticleOrder_by(2);
    */
  }

  public Particle[] CalcParticleOrder_by(int orderedBy)
  {
    Particle[] sorted = this.Particles.OrderBy(p => p.Position[orderedBy]).ToArray();
    for(int i = 0; i < sorted.Length; i++)
    {
      sorted[i].OrderIndex[orderedBy] = i;
    }
    return sorted;
  }

  public void CalcFitness()
  {
    foreach(Particle particle in this.Particles)
    {
      particle.CalcFitness();
      if(particle.BestFitness > this.BestFitness)
      {
        this.BestFitness = particle.BestFitness;
        this.BestPosition = particle.BestPosition;
      }
    }
  }

  public void UpdateParticleSpeed()
  {
    foreach(Particle particle in this.Particles)
    {
      particle.SetSpeed(
        SwarmSetting.INTERIA_FACTOR * particle.Speed
        + SwarmSetting.COGNITIVE_WEIGHT * (particle.BestPosition - particle.Position)
          * SwarmSetting.sigmoid(particle.BestFitness - particle.Fitness)
        + SwarmSetting.SOCIAL_WEIGHT * (this.BestPosition - particle.Position)
          * SwarmSetting.sigmoid(this.BestFitness - particle.Fitness)
      );
    }
  }

  public void Update()
  {
    this.CalcFitness();
    this.UpdateParticleSpeed();
    this.CalcParticleOrder();
  }
}
