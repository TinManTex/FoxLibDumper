using FmdlStudio.Scripts.Classes;
using System;
using System.Collections.Generic;
using System.IO;

namespace FoxLibDumper.OtherLoaders
{
    class FmdlLoader
    {
        //tex strip file extension and convert to hex string
        static string PathFileNameCodeToPathCodeStr(ulong hash)
        {
            return (hash - 0x1568000000000000).ToString("x");
        }

        public static void ReadHashes(string filePath, ref Dictionary<string, HashSet<string>> hashes, ref List<string> failed)
        {
            string fmdlName = Path.GetFileNameWithoutExtension(filePath);
            Fmdl fmdl = new Fmdl(fmdlName);
            fmdl.Read(filePath);

            bool isGZFormat = fmdl.version == 2.03f;
            if (isGZFormat)
            {
                return;
            }

            int boneCount = fmdl.fmdlBones != null ? fmdl.fmdlBones.Length : 0;
            int materialInstanceCount = fmdl.fmdlMaterialInstances.Length;
            int textureCount = fmdl.fmdlTextures != null ? fmdl.fmdlTextures.Length : 0;
            int meshCount = fmdl.fmdlMeshInfos.Length;
            int meshGroupCount = fmdl.fmdlMeshGroups.Length;
            int materialCount = fmdl.fmdlMaterials.Length;
            int boneGroupCount = fmdl.fmdlBoneGroups != null ? fmdl.fmdlBoneGroups.Length : 0;

            for (int i = 0; i < boneCount; i++)
            {
                Fmdl.FmdlBone fmdlBone = fmdl.fmdlBones[i];
                hashes["BoneName"].Add(fmdl.fmdlStrCode64s[fmdlBone.nameIndex].ToString());
            } //for boneCount

            for (int i = 0; i < textureCount; i++)
            {
                Fmdl.FmdlTexture fmdlTexture = fmdl.fmdlTextures[i];

                var hash = fmdl.fmdlPathCode64s[fmdlTexture.pathIndex];
                string hashStr = PathFileNameCodeToPathCodeStr(hash);
                hashes["TexturePath"].Add(hashStr);

                //hashes["TexturePath"].Add(fmdl.fmdlPathCode64s[fmdlTexture.pathIndex].ToString());

                //tex we're not sure what these are for, in gz it has seperate strings for path and filename, but tpp hash full path/filename in fmdlPathCode64s[fmdlTexture.pathIndex]
                hashes["TextureName"].Add(fmdl.fmdlStrCode64s[fmdlTexture.nameIndex].ToString());
            } //for textureCount

            for (int i = 0; i < materialInstanceCount; i++)
            {
                Fmdl.FmdlMaterialInstance fmdlMaterialInstance = fmdl.fmdlMaterialInstances[i];
                int materialIndex = fmdlMaterialInstance.materialIndex;
                int typeIndex = fmdl.fmdlMaterials[materialIndex].typeIndex;
                hashes["ShaderName"].Add(fmdl.fmdlStrCode64s[typeIndex].ToString());
                hashes["MaterialInstance"].Add(fmdl.fmdlStrCode64s[fmdlMaterialInstance.nameIndex].ToString());

                for (int j = fmdlMaterialInstance.firstTextureIndex; j < fmdlMaterialInstance.firstTextureIndex + fmdlMaterialInstance.textureCount; j++)
                {
                    Fmdl.FmdlMaterialParameter fmdlMaterialParameter = fmdl.fmdlMaterialParameters[j];
                    hashes["TextureType"].Add(fmdl.fmdlStrCode64s[fmdlMaterialParameter.nameIndex].ToString());
                } //for texture types

                for (int j = fmdlMaterialInstance.firstParameterIndex; j < fmdlMaterialInstance.firstParameterIndex + fmdlMaterialInstance.parameterCount; j++)
                {
                    Fmdl.FmdlMaterialParameter fmdlMaterialParameter = fmdl.fmdlMaterialParameters[j];
                    hashes["MaterialParameter"].Add(fmdl.fmdlStrCode64s[fmdlMaterialParameter.nameIndex].ToString());
                } //for parameters
            } //for materialInstanceCount        

            for (int i = 0; i < meshGroupCount; i++)
            {
                Fmdl.FmdlMeshGroup fmdlMeshGroup = fmdl.fmdlMeshGroups[i];
                hashes["MeshGroup"].Add(fmdl.fmdlStrCode64s[fmdlMeshGroup.nameIndex].ToString());
            } //for meshGroupCount

            for (int i = 0; i < meshCount; i++)
            {
                int meshGroupIndex = Array.Find(fmdl.fmdlMeshGroupEntries, x => x.firstMeshIndex <= i && x.firstMeshIndex + x.meshCount > i).meshGroupIndex;//tex wat
                int nameIndex = fmdl.fmdlMeshGroups[meshGroupIndex].nameIndex;
                hashes["MeshName"].Add(fmdl.fmdlStrCode64s[nameIndex].ToString());
            } //for meshCount

            for (int i = 0; i < materialCount; i++)
            {
                Fmdl.FmdlMaterial material = fmdl.fmdlMaterials[i];

                hashes["MaterialName"].Add(fmdl.fmdlStrCode64s[material.nameIndex].ToString());
                hashes["MaterialType"].Add(fmdl.fmdlStrCode64s[material.typeIndex].ToString());
            } // for materialCount

            //
            foreach (var hash in fmdl.fmdlStrCode64s)
            {
                //hashes["StrCode64Section"].Add(hash.ToString());
            }
        }//ReadHashes
    }//FmdlLoader
}
