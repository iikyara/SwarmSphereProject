using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Particle
{
  //params
  public Vector3 Position;
  public float Scale;
  public Vector3 Speed;
  public float Fitness;
  public FitnessFunction FitnessFunction;
  public Vector3 BestPosition;
  public float BestFitness;
  public Vector3Int OrderIndex;

  public Particle(
    Vector3 initialPosition,
    float initialScale,
    Vector3 initialSpeed,
    FitnessFunction fitnessFunction
  )
  {
    this.Position = initialPosition;
    this.Scale = initialScale;
    this.Speed = initialSpeed;
    this.Fitness = 0.0f;
    this.FitnessFunction = fitnessFunction;
    this.BestPosition = new Vector3();
    this.BestFitness = float.MinValue;
    this.OrderIndex = new Vector3Int();
  }

  /// <summary>
  /// コピーコンストラクタ
  /// </summary>
  /// <param name="instance"></param>
  public Particle(Particle instance)
  {
    this.Position = instance.Position;
    this.Scale = instance.Scale;
    this.Speed = instance.Speed;
    this.Fitness = instance.Fitness;
    this.FitnessFunction = instance.FitnessFunction;
    this.BestPosition = instance.BestPosition;
    this.BestFitness = instance.BestFitness;
    this.OrderIndex = instance.OrderIndex;
}

  public void SetSpeed(Vector3 speed)
  {
    this.Speed = speed;
  }

  public void SetFitness(float fitness)
  {
    this.Fitness = fitness;
    if (fitness > this.BestFitness)
    {
      this.BestFitness = fitness;
      this.BestPosition = this.Position;
    }
  }

  public void SetFitnessFunction(FitnessFunction fitnessFunction)
  {
    this.FitnessFunction = fitnessFunction;
  }

  public void CalcFitness()
  {
    float fitness = this.FitnessFunction.getFitness(this);
    this.SetFitness(fitness);
    this.Position += this.Speed;
  }

  public void UpdateParticleSpeed()
  {
    this.SetSpeed(
      SwarmSetting.INTERIA_FACTOR * this.Speed
      + SwarmSetting.COGNITIVE_WEIGHT * (this.BestPosition - this.Position)
    );
  }

  public void Update()
  {
    this.CalcFitness();
    this.UpdateParticleSpeed();
  }
}
