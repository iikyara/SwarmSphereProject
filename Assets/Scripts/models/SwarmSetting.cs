using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class SwarmSetting : MonoBehaviour
{
  //params
  public static Vector3 PARTICLE_UPPER_BOUNDS = new Vector3(1f, 1f, 1f);
  public static Vector3 PARTICLE_LOWER_BOUNDS = new Vector3(-1f, -1f, -1f);

  public static Vector3 PARTICLE_UPPER_BOUNDS_POSITION = PARTICLE_UPPER_BOUNDS;
  public static Vector3 PARTICLE_LOWER_BOUNDS_POSITION = PARTICLE_LOWER_BOUNDS;

  public static Vector3 PARTICLE_UPPER_BOUNDS_SPEED = PARTICLE_UPPER_BOUNDS;
  public static Vector3 PARTICLE_LOWER_BOUNDS_SPEED = PARTICLE_LOWER_BOUNDS;

  public static float INTERIA_FACTOR = 0.99f;
  public static float COGNITIVE_WEIGHT = 0.1f;
  public static float SOCIAL_WEIGHT = 0.001f;
  public static float GLOBAL_WEIGHT = 0f;

  public static float RANGE_OF_DISTANCE_BETWEEN_PARTICLES = 20f;

  public static float sigmoid(float x, float a = 5f)
  {
    return 1f / (1f + (float)Math.Exp(-a * x));
  }
}
