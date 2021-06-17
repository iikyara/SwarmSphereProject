using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControllerSetting : MonoBehaviour
{
  public Material ExistentialParticleMaterial;
  public Material NonExistentialParticleMaterial;
  public Material RelationEdgeMaterial;
  public Material EdgeMaterial;
  public Material MeshMaterial;

  public Material BaseMeshMaterial;
  public Material CaptureMaterial;
  public Material DisabledParticleMaterial;
  //public GameObject ParentObject;

  private void Awake()
  {
    MaterialSetup();
  }

  public void MaterialSetup()
  {
    //Primitive.SetBaseParent()
    Primitive.SetBaseMaterial(ExistentialParticleMaterial, NonExistentialParticleMaterial, DisabledParticleMaterial);
    VoronoiPartition.SetBaseMaterials(RelationEdgeMaterial, EdgeMaterial, BaseMeshMaterial, ExistentialParticleMaterial, NonExistentialParticleMaterial, DisabledParticleMaterial);
    VoronoiPartitionFast.SetBaseMaterials(RelationEdgeMaterial, EdgeMaterial, BaseMeshMaterial, ExistentialParticleMaterial, NonExistentialParticleMaterial, DisabledParticleMaterial);
    Primitive.CombinedVoronoiPartition.LoadBaseMaterials();
    Primitive.CombinedVoronoiPartition.meshMaterial = MeshMaterial;
    LPIndividual.SetMaterial(BaseMeshMaterial, CaptureMaterial);
    AgentLocationProblemSolver.SetMaterial(CaptureMaterial, BaseMeshMaterial);
    ALPIndividual.SetMaterial(ExistentialParticleMaterial, NonExistentialParticleMaterial);
    //VoronoiPartition.SetBaseParent()
  }
}
