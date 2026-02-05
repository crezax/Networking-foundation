using Google.FlatBuffers;
using UnityEngine;

namespace Transport.Messages {
  public static class ExtensionMethods {
    public static Offset<TVector3> ToTransport(
        this Vector3 vector,
        FlatBufferBuilder builder) {
      return TVector3.CreateTVector3(
          builder,
          vector.x,
          vector.y,
          vector.z);
    }

    public static Vector3 ToVector3(this TVector3 quaternion) {
      return new Vector3(
          quaternion.X,
          quaternion.Y,
          quaternion.Z);
    }

    public static Offset<TQuaternion> ToTransport(
        this Quaternion quaternion,
        FlatBufferBuilder builder) {
      return TQuaternion.CreateTQuaternion(
          builder,
          quaternion.x,
          quaternion.y,
          quaternion.z,
          quaternion.w);
    }

    public static Quaternion ToQuaternion(this TQuaternion quaternion) {
      return new Quaternion(
          quaternion.X,
          quaternion.Y,
          quaternion.Z,
          quaternion.W);
    }
  }
}