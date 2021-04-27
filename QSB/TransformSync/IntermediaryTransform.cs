using QSB.SectorSync.WorldObjects;
using UnityEngine;

namespace QSB.TransformSync
{
	class IntermediaryTransform
	{
		private Transform _attachedTransform;
		private QSBSector _referenceSector;

		public IntermediaryTransform(Transform transform)
		{
			_attachedTransform = transform;
		}
		/// <summary>
		/// Get the world position of this INVISIBLE transform.
		/// </summary>
		public Vector3 GetPosition() 
			=> _attachedTransform.position;

		/// <summary>
		/// Set the world position of this INVISIBLE transform.
		/// </summary>
		public void SetPosition(Vector3 worldPos) 
			=> _attachedTransform.position = worldPos;

		/// <summary>
		/// Get the world rotation of this INVISIBLE transform.
		/// </summary>
		public Quaternion GetRotation() 
			=> _attachedTransform.rotation;

		/// <summary>
		/// Set the world rotation of this INVISIBLE transform.
		/// </summary>
		public void SetRotation(Quaternion worldRot) 
			=> _attachedTransform.rotation = worldRot;

		/// <summary>
		/// Sets the reference sector - what sector this transform is syncing to.
		/// </summary>
		/// <param name="sector">The new reference sector.</param>
		public void SetReferenceSector(QSBSector sector) 
			=> _referenceSector = sector;

		/// <summary>
		/// Sets the position of the INVISIBLE transform to be correct, according to the reference sector and the position of the VISIBLE transform.
		/// </summary>
		/// <param name="worldPosition">The world position of the VISIBLE transform.</param>
		public void EncodePosition(Vector3 worldPosition) 
			=> SetPosition(_referenceSector.Transform.InverseTransformPoint(worldPosition));

		/// <summary>
		/// Sets the rotation of the INVISIBLE transform to be correct, according to the reference sector and the rotation of the VISIBLE transform.
		/// </summary>
		/// <param name="worldPosition">The world rotation of the VISIBLE transform.</param>
		public void EncodeRotation(Quaternion worldRotation)
			=> SetRotation(_referenceSector.Transform.InverseTransformRotation(worldRotation));

		/// <summary>
		/// Gets what the VISIBLE transform's position should be, from the reference sector and the position of the INVISIBLE transform.
		/// </summary>
		public Vector3 GetTargetPosition()
			=> GetPosition();

		/// <summary>
		/// Gets what the VISIBLE transform's rotation should be, from the reference sector and the rotation of the INVISIBLE transform.
		/// </summary>
		public Quaternion GetTargetRotation()
			=> GetRotation();
	}
}
