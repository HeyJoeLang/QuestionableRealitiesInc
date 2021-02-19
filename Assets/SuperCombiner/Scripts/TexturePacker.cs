using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace LunarCatsStudio.SuperCombiner {

    /// <summary>
    /// This class manages the creation of the Atlas texture
    /// </summary>
    public class TexturePacker  {

        /// <summary>
        /// List of combined _material
        /// </summary>
		public Material _copyedMaterials;
        /// <summary>
        /// List of combined _material for saving  
        /// </summary>
        public Material _copyedToSaveMaterials;
        
        // List of original textures for each texture property
        private Dictionary<string, List<Texture2D>> _texturesForAtlas = new Dictionary<string, List<Texture2D>> ();
		// Packed textures associated with each texture property
		public Dictionary<string, Texture2D> _packedTextures = new Dictionary<string, Texture2D> ();
        /// <summary>
        /// The combined _index, used to differenciate the combined materials when using multiple _material function
        /// </summary>
        private int _combinedIndex = 0;
        /// <summary>
        /// The _index of this instance in the list of TexturePacker 
        /// </summary>
        public int CombinedIndex
        {
            get
            {
                return _combinedIndex;
            }
            set
            {
                _combinedIndex = value;
            }
        }

		// List of texture property _names in shaders
		public Dictionary<string, string> TexturePropertyNames = new Dictionary<string, string> {
			{"_MainTex", "Diffuse"},
			{"_BumpMap", "Normal"},
			{"_SpecGlossMap", "Specular"},
			{"_ParallaxMap", "Height"},
			{"_OcclusionMap", "Occlusion"},
			{"_EmissionMap", "Emission"},
			{"_DetailMask", "Detail Mask"},
			{"_DetailAlbedoMap", "Detail Diffuse"},
			{"_DetailNormalMap", "Detail Normal"},
			{"_MetallicGlossMap", "Metallic"},
			{"_LightMap", "Light Map"}
		};

        // True if this material has emission
        private bool hasEmission = false;
        // The emission color
        private Color emissionColor = Color.black;

        // The list of custom properties name
        private List<string> customProperties = new List<string>();
        // List of all textures used in all children game objects
        private Dictionary<int, TextureImportSettings> importedTextures = new Dictionary<int, TextureImportSettings>();

        // Flags indicating if warning have already been registered
        private List<int> uvOutOfBoundWarnMesh = new List<int>();
        private bool differentMaterialWarning = false;

        // Default size of color texture packed when no main texture found in a _material
        private const int NO_TEXTURE_COLOR_SIZE = 256;
		// The maximum texture size handled by Unity
		private const int MAX_TEXTURE_SIZE = 16384;

        // The _combinedResult reference
        private CombinedResult combinedResult;
        public CombinedResult CombinedResult
        {
            set
            {
                combinedResult = value;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public TexturePacker() {		
			foreach (KeyValuePair <string, string> keyValue in TexturePropertyNames) {
				_texturesForAtlas.Add (keyValue.Key, new List<Texture2D> ());
			}
		}

		public void SetCustomPropertyNames(List<string> list) {
			foreach(string property in list) {
				if(!TexturePropertyNames.ContainsKey(property)) {
					TexturePropertyNames.Add(property, property);
                    customProperties.Add(property);
                    _texturesForAtlas.Add(property, new List<Texture2D>());
                }
			}
		}

		public Material GetCombinedMaterialToSave() {
            return _copyedToSaveMaterials;
        }

        public void GenerateCopyedMaterialToSave() {
            Material mat = new Material(_copyedMaterials);
            _copyedToSaveMaterials = mat;
        }

        /// <summary>
        /// Set the Copied Material value
        /// </summary>
        /// <param name="mat"></param>
        public void SetCopiedMaterial(Material mat)
        {
            _copyedMaterials = mat;
        }

        /// <summary>
        /// Clear all the texture data
        /// </summary>
        public void ClearTextures () {		
			_packedTextures.Clear ();
            _texturesForAtlas.Clear();

            foreach(string property in customProperties) {
                TexturePropertyNames.Remove(property);
            }
            customProperties.Clear();
			foreach (KeyValuePair <string, string> keyValue in TexturePropertyNames) {
				_texturesForAtlas.Add (keyValue.Key, new List<Texture2D> ());
			}
            
            importedTextures.Clear();
            uvOutOfBoundWarnMesh.Clear();
            differentMaterialWarning = false;

            hasEmission = false;
            emissionColor = Color.black;
        }

        /// <summary>
        /// Returns the size of the texture in the atlas. If the size exceeds the maximum texture size, it will be reduced
        /// </summary>
        /// <param name="inputTextureSize"></param>
        /// <param name="scaleX"></param>
        /// <param name="scaleY"></param>
        /// <param name="materialName"></param>
        /// <returns></returns>
		private Vector3 GetTextureSizeInAtlas(Vector2 inputTextureSize, MaterialToCombine materialToCombine) {
            Rect uvBounds = materialToCombine.GetScaledAndOffsetedUVBounds();
            float scaleX = Mathf.Abs(uvBounds.width);
            float scaleY = Mathf.Abs(uvBounds.height);
            Vector3 size = new Vector3 (inputTextureSize.x * scaleX, inputTextureSize.y * scaleY, 1);

			if (size.x >= MAX_TEXTURE_SIZE || size.y >= MAX_TEXTURE_SIZE) {
				// If the tilled texture exceed the maximum texture size handled by Unity, we need to reduce it's size
				int reducingFactor = (int) Mathf.Max (Mathf.Ceil (size.x / SystemInfo.maxTextureSize), Mathf.Ceil (size.y / SystemInfo.maxTextureSize));
                Logger.Instance.AddLog("SuperCombiner", "Textures in material '" + materialToCombine._material.name + "' are being tiled and the total tiled size exceeds the maximum texture size for the current plateform (" + SystemInfo.maxTextureSize + "). All textures in this _material will be shrunk by " + reducingFactor + " to fit in the atlas. This could leads to a quality loss. Whenever possible, avoid combining tiled texture.", Logger.LogLevel.LOG_WARNING);
                if (!uvOutOfBoundWarnMesh.Contains(materialToCombine._meshHavingBiggestUVBounds.GetInstanceID()))
                {
                    Debug.Log(materialToCombine._meshHavingBiggestUVBounds.GetInstanceID());
                    combinedResult.AddWarningMessage("The mesh '" + materialToCombine._meshHavingBiggestUVBounds.name + "' has UVs way out of [0, 1] bounds, actual bounds are [" + uvBounds.width + ", " + uvBounds.height + "]\n"
                        + "Textures associated to this mesh are therefore being tiled by " + uvBounds.width + " in width and " + uvBounds.height + " in height. Given a texture of size " + inputTextureSize
                        + ", it's tiled size will be (" + size.x + ", " + size.y + "), which is too big (the max. texture size is (" + MAX_TEXTURE_SIZE + ", " + MAX_TEXTURE_SIZE + "))\n"
                        + "This leads to quality loss in the atlas texture. You should consider editing the UV of mesh '" + materialToCombine._meshHavingBiggestUVBounds.name + "' with a 3D modeler tool to avoid this problem."
                        );
                    uvOutOfBoundWarnMesh.Add(materialToCombine._meshHavingBiggestUVBounds.GetInstanceID());
                }

                size.Set(size.x / reducingFactor, size.y / reducingFactor, reducingFactor);
            }
			return size;
		}

        /// <summary>
        /// Returns the texture that will be put in the atlas.
        /// This texture may differs from original texture in case of tiling, or if texture needs to be resized
        /// </summary>
        /// <param name="texture"></param>
        /// <param name="materialUVBounds"></param>
        /// <param name="meshUVBounds"></param>
        /// <param name="mat"></param>
        /// <param name="textureInAtlasSize"></param>
        /// <param name="targetTextureSize"></param>
        /// <param name="isMainTexture"></param>
        /// <returns></returns>
		private Texture2D CopyTexture(Texture2D texture, Rect materialUVBounds, Rect meshUVBounds, Material mat, Vector3 textureInAtlasSize, Vector2 targetTextureSize, bool isMainTexture) {
			int xSens = (int) Mathf.Sign (materialUVBounds.width);
			int ySens = (int) Mathf.Sign (materialUVBounds.height);
			float repeatX = Mathf.Abs(materialUVBounds.width);
			float repeatY = Mathf.Abs(materialUVBounds.height);
			Color mainColor = Color.white;

			if (mat.HasProperty ("_Color")) {
				mainColor = mat.color;
			}

			bool hasUVoutOfBound = false;
			if (repeatX != 1 || repeatY != 1 || materialUVBounds.position != Vector2.zero) {
				hasUVoutOfBound = true;
			}

            // Check Texture import settings
            TextureImportSettings importSettings = CheckTextureImportSettings (texture);

            // If an error occured, return a simple white texture
            if (!importSettings.isReadable)
            {
                Logger.Instance.AddLog("SuperCombiner", "The format of texture '" + texture.name + "' is not handled by Unity. Try manually setting 'Read/Write Enabled' parameter to true or converting this texture into a known format.",Logger.LogLevel.LOG_ERROR);
                return CreateColoredTexture2D((int)textureInAtlasSize.x, (int)textureInAtlasSize.y, Color.white);
            }

            // Get a copy of the original texture so that it remains intact
            Texture2D uncompressedTexture = new Texture2D(texture.width, texture.height, TextureFormat.RGBA32, false);
			uncompressedTexture.name = texture.name;
            // Copy original pixels /!\ Only possible if the texture could be red correctly
            uncompressedTexture.SetPixels(texture.GetPixels());

            if (texture.width != (int) targetTextureSize.x || texture.height != (int) targetTextureSize.y) {
				// Scale the texture to it's max size so that all textures from this _material share the same texture size
				TextureScale.Bilinear(uncompressedTexture, (int) targetTextureSize.x, (int) targetTextureSize.y);
                Logger.Instance.AddLog("SuperCombiner", "Texture '" + texture.name + "' will be scaled from " + GetStringTextureSize(texture.width, texture.height) + " to " + GetStringTextureSize(targetTextureSize.x, targetTextureSize.y) + " to match the size of the other textures in _material '" + mat.name + "'",Logger.LogLevel.LOG_WARNING);
			}

			// Create a new Texture2D which will contain the adjusted texture tilled and scaled
			Texture2D copy = new Texture2D((int) textureInAtlasSize.x, (int) textureInAtlasSize.y, uncompressedTexture.format, false);	
			copy.name = texture.name;
			
            // If the tiled texture exceeds the maximum texture size, we have to shrink the texture
			if (textureInAtlasSize.z != 1 && textureInAtlasSize.z > 0) {
				TextureScale.Bilinear (uncompressedTexture, uncompressedTexture.width / (int) textureInAtlasSize.z, uncompressedTexture.height / (int) textureInAtlasSize.z);
			}

			if (hasUVoutOfBound)
            {
				if (Mathf.Abs (textureInAtlasSize.x - uncompressedTexture.width) > 1 || Mathf.Abs (textureInAtlasSize.y - uncompressedTexture.height) > 1) {
                    Logger.Instance.AddLog("SuperCombiner", "Texture '" + texture.name + "' is being tiled in the atlas because mesh using it has UVs out of [0, 1] bound. The tiled size is " + GetStringTextureSize (textureInAtlasSize.x, textureInAtlasSize.y) + ".");
				}
				// If UVs are out of [0, 1] bound we have to duplicate the texture
				int xOffset = (int)(meshUVBounds.xMin * uncompressedTexture.width * mat.mainTextureScale.x + mat.mainTextureOffset.x * uncompressedTexture.width);
				int yOffset = (int)(meshUVBounds.yMin * uncompressedTexture.height * mat.mainTextureScale.y + mat.mainTextureOffset.y * uncompressedTexture.height);

				int i = 0, j = 0;
				if (xSens < 0 || ySens < 0 || ((!mainColor.Equals (Color.white)) && isMainTexture)) {
					for (i = 0; i < copy.width; i++) {
						for (j = 0; j < copy.height; j++) {
							copy.SetPixel (i, j, uncompressedTexture.GetPixel (xSens * (i + xOffset) % uncompressedTexture.width, ySens * (j + yOffset) % uncompressedTexture.height) * mainColor);
						}
					}
				} else {
					int blockWidth = 0, blockHeight = 0;
					while (i < copy.width) {
						int posx = (xSens * (i + xOffset) % uncompressedTexture.width + uncompressedTexture.width) % uncompressedTexture.width;
						blockWidth = (i + uncompressedTexture.width <= copy.width) ? uncompressedTexture.width : copy.width - i;
						if (posx + blockWidth > uncompressedTexture.width) {
							blockWidth = uncompressedTexture.width - posx;
						}
						while (j < copy.height) {					
							int posy = (ySens * (j + yOffset) % uncompressedTexture.height + uncompressedTexture.height) % uncompressedTexture.height;
							blockHeight = (j + uncompressedTexture.height <= copy.height) ? uncompressedTexture.height : copy.height - j;
							if (posy + blockHeight > uncompressedTexture.height) {
								blockHeight = uncompressedTexture.height - posy;
							}
							copy.SetPixels (i, j, blockWidth, blockHeight, uncompressedTexture.GetPixels (posx, posy, blockWidth, blockHeight));
							j += blockHeight;
						}
						j = 0;
						i += blockWidth;
					}
				}
			}
            else
            {
				if (mainColor.Equals (Color.white) || !isMainTexture)
                {
					// UVs are all inside [0, 1] bound, so we can fast copy this texture
					copy.LoadRawTextureData (uncompressedTexture.GetRawTextureData ());
				}
                else
                {
					for (int i = 0; i < copy.width; i++)
                    {
						for (int j = 0; j < copy.height; j++)
                        {
							copy.SetPixel (i, j, uncompressedTexture.GetPixel (i, j) * mainColor);
						}
					}
				}
			}
			return copy;
		}

		/// <summary>
		/// Checks if all textures in the given _material has the same size.
		/// The returned sized is the smallest/biggest.
		/// </summary>
		/// <returns>The textures size.</returns>
		/// <param name="mat">Mat.</param>
		/// <param name="alignToSmallest">If set to <c>true</c> align to biggest.</param>
		private Vector2 checkTexturesSize(Material mat, bool alignToSmallest) {
			Vector2 maxSize = Vector2.zero;

			foreach (string textureProperty in TexturePropertyNames.Keys) {
				if (mat.HasProperty (textureProperty)) {
					Texture texture = mat.GetTexture (textureProperty);
					if (texture != null) {
						if (texture.width != maxSize.x || texture.height != maxSize.y) {
							if (alignToSmallest) {
                                // Align to smallest
                                if (maxSize != Vector2.zero)
                                {
                                    Logger.Instance.AddLog("SuperCombiner", "Material '" + mat.name + "' has various textures with different size! Textures in this _material will be scaled to match the smallest one.\nTo avoid this, ensure to have all textures in a _material of the same size. Try adjusting 'Max Size' in import settings.", Logger.LogLevel.LOG_WARNING);
                                }
								if (maxSize == Vector2.zero || texture.width * texture.height < maxSize.x * maxSize.y) {
									maxSize = new Vector2 (texture.width, texture.height);
								}
							} else {
                                // Align to biggest
                                if (maxSize != Vector2.zero)
                                {
                                    Logger.Instance.AddLog("SuperCombiner", "Material '" + mat.name + "' has various textures with different size! Textures in this _material will be scaled to match the biggest one.\nTo avoid this, ensure to have all textures in a _material of the same size. Try adjusting 'Max Size' in import settings.", Logger.LogLevel.LOG_WARNING);
                                }
                                if (maxSize == Vector2.zero || texture.width * texture.height > maxSize.x * maxSize.y) {
									maxSize = new Vector2 (texture.width, texture.height);
								}
							}
						}
					}
				}
			}

			if (maxSize == Vector2.zero) {
				// If no texture found, returned a default texture size
				maxSize = new Vector2 (NO_TEXTURE_COLOR_SIZE, NO_TEXTURE_COLOR_SIZE);
			}
			
			return maxSize;
		}

        /// <summary>
        /// Process a new _material, adding all textures found based on the _material property list in the lists for the atlas
        /// </summary>
        /// <param name="mat">Mat.</param>
        /// <param name="combineMaterials">If set to <c>true</c> combine materials.</param>
        /// <param name="materialUVBounds">Material UV bounds.</param>
        /// <param name="meshUVBounds">Mesh UV bounds.</param>
        /// <param name="tilingFactor">Tilling factor.</param>
        //public void SetTextures(Material mat, bool combineMaterials, Rect materialUVBounds, Rect meshUVBounds, float tilingFactor)
        public void SetTextures(Material mat, bool combineMaterials, MaterialToCombine materialToCombine, float tilingFactor)
        {
            // Here we have to check if all textures have the same size, otherwise, rescale the texture with the biggest one
            Vector2 maxTextureSize = checkTexturesSize (mat, false);

            Rect materialUVBounds = materialToCombine.GetScaledAndOffsetedUVBounds();
            Rect meshUVBounds = materialToCombine._uvBounds;

            // Manage tiling factor
            if (tilingFactor > 1)
            {				
				combinedResult._combinedMaterials[_combinedIndex].scaleFactors.Add(tilingFactor);
                materialUVBounds.size = Vector2.Scale(materialUVBounds.size, Vector2.one * tilingFactor);
                meshUVBounds.position -= new Vector2(meshUVBounds.width * (tilingFactor - 1) / 2f, meshUVBounds.height * (tilingFactor - 1) / 2f);
            }
            else
            {
				combinedResult._combinedMaterials[_combinedIndex].scaleFactors.Add(1);
            }

            combinedResult._combinedMaterials[_combinedIndex].meshUVBounds.Add(meshUVBounds);

            // Calculate the tiled texture size
            Vector3 textureInAtlasSize = GetTextureSizeInAtlas(maxTextureSize, materialToCombine);

			foreach(KeyValuePair<string, List<Texture2D>> keyValue in _texturesForAtlas) {
				if (mat.HasProperty (keyValue.Key)) {
					Texture texture = mat.GetTexture (keyValue.Key);
					if (texture != null) {
						Texture2D textureInAtlas = CopyTexture ((Texture2D)texture, materialUVBounds, meshUVBounds, mat, textureInAtlasSize, maxTextureSize, keyValue.Key.Equals ("_MainTex"));
						textureInAtlasSize = new Vector2(textureInAtlas.width, textureInAtlas.height);
						keyValue.Value.Add (textureInAtlas );

                        // If texture is normal, revert modification in import settings of original texture
                        if (importedTextures.ContainsKey(texture.GetInstanceID()))
                        {
                            if (importedTextures[texture.GetInstanceID()].isNormal)
                            {
#if UNITY_EDITOR
                                string path = AssetDatabase.GetAssetPath(texture);
                                TextureImporter textureImporter = AssetImporter.GetAtPath(path) as TextureImporter;
#if UNITY_2017_1_OR_NEWER
                                textureImporter.textureType = TextureImporterType.NormalMap;
#else
                                textureImporter.textureType = TextureImporterType.Bump;
#endif
                                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
#endif
                            }
                        }
                    } else {
						if (keyValue.Key.Equals ("_MainTex"))
                        {
							keyValue.Value.Add (CreateColoredTexture2D ((int)textureInAtlasSize.x, (int)textureInAtlasSize.y, mat.HasProperty ("_Color") ? mat.color : Color.white));
                            Logger.Instance.AddLog("SuperCombiner", "Creating a colored texture " + (mat.HasProperty ("_Color") ? mat.color : Color.white) + " of size " + GetStringTextureSize(textureInAtlasSize.x, textureInAtlasSize.y) + " for " + TexturePropertyNames[keyValue.Key] + " in _material '" + mat.name + "' because texture is missing.");
						}
                        else if (keyValue.Key.Equals("_EmissionMap") && mat.IsKeywordEnabled("_EMISSION") && mat.GetColor("_EmissionColor") != Color.black)
                        {
                            keyValue.Value.Add(CreateColoredTexture2D((int)textureInAtlasSize.x, (int)textureInAtlasSize.y, Color.white));

                            hasEmission = true;
                            emissionColor = mat.GetColor("_EmissionColor");
                            Logger.Instance.AddLog("SuperCombiner", "Creating a white texture of size " + GetStringTextureSize(textureInAtlasSize.x, textureInAtlasSize.y) + " for " + TexturePropertyNames[keyValue.Key] + " in material '" + mat.name + "' because texture is missing.");
                        }
                        else if (keyValue.Value.Count > 0) {
							// Add color texture because this texture is missing in _material
							keyValue.Value.Add (CreateColoredTexture2D ((int) textureInAtlasSize.x, (int)textureInAtlasSize.y, DefaultColoredTexture.GetDefaultTextureColor(keyValue.Key)));
                            Logger.Instance.AddLog("SuperCombiner", "Creating a colored texture " + DefaultColoredTexture.GetDefaultTextureColor (keyValue.Key) + " of size " + GetStringTextureSize(textureInAtlasSize.x, textureInAtlasSize.y) + " for " + TexturePropertyNames[keyValue.Key] + " in _material '" + mat.name + "' because texture is missing.");
						}
					}
				} else {
					// The _material doesn't have this texture property
					if (keyValue.Key.Equals ("_MainTex")) {
						keyValue.Value.Add (CreateColoredTexture2D ((int)maxTextureSize.x, (int)maxTextureSize.y, mat.HasProperty("_Color") ? mat.color : Color.white));
                        Logger.Instance.AddLog("SuperCombiner", "Creating a colored texture " + DefaultColoredTexture.GetDefaultTextureColor (keyValue.Key) + " of size " + GetStringTextureSize(textureInAtlasSize.x, textureInAtlasSize.y) + " for " + TexturePropertyNames[keyValue.Key] + " in _material '" + mat.name + "' because texture is missing.");
					} else if (keyValue.Value.Count > 0) {
                        // Error here because we found materials with textures properties that don't match!
                        Logger.Instance.AddLog("SuperCombiner", "Found materials with properties that don't match. Material '" + mat.name + "' do not have the property " + keyValue.Key + " whereas others material don't, combining materials with differents properties together may lead to inconsistent results.", Logger.LogLevel.LOG_WARNING);
                        if (!differentMaterialWarning)
                        {
                            combinedResult.AddWarningMessage("Several materials where found that don't share the same property, this can lead to inconsistent results. You should always combine materials of the same type or which share the same properties, consider splitting your GameObjects to combine by types of materials or use multi material feature to group them.");
                            differentMaterialWarning = true;
                        }
					}
				}
			}
		}

		private string GetStringTextureSize(float width, float height) {
			return (int) width + "x" + (int) height + " pixels";
		}

		/// <summary>
		/// Creates a colored texture2d.
		/// </summary>
		/// <returns>The colored texture2 d.</returns>
		/// <param name="width">Width.</param>
		/// <param name="height">Height.</param>
		/// <param name="color">Color.</param>
		private Texture2D CreateColoredTexture2D(int width, int height, Color color) {
			Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
			for(int i=0; i<width; i++) {
				for(int j=0; j<height; j++) {
					tex.SetPixel(i, j, color);
				}
			}
			tex.Apply();
			return tex;
		}

        TextureImportSettings CheckTextureImportSettings(Texture2D texture)
        {
            TextureImportSettings importSettings;

#if UNITY_EDITOR
            string path = AssetDatabase.GetAssetPath(texture);
            // Check if textures settings has already been setted up
            if (importedTextures.ContainsKey(texture.GetInstanceID()))
            {
                importSettings = importedTextures[texture.GetInstanceID()];
                if (!importSettings.isNormal)
                {
                    return importSettings;
                }
            }
            else
            {
                importSettings = new TextureImportSettings();
            }

            TextureImporter textureImporter = AssetImporter.GetAtPath(path) as TextureImporter;
            if (textureImporter != null)
            {
                if (!textureImporter.isReadable)
                {
                    textureImporter.isReadable = true;
                    AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
                }

#if UNITY_2017_1_OR_NEWER
                if (textureImporter.textureType == TextureImporterType.NormalMap)
                {
                    textureImporter.textureType = TextureImporterType.Default;
                    AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
                    importSettings.isNormal = true;
                }
#endif
            }
            else
            {
                try
                {
                    texture.GetPixel(0, 0);
                }
                catch
                {
                    importSettings.isReadable = false;
                    Logger.Instance.AddLog("SuperCombiner", "The texture '" + texture.name + "' has a format not handled by Unity, the result in atlas may be inconsistent.", Logger.LogLevel.LOG_WARNING);
                }
            }
#else
            importSettings = new TextureImportSettings();
#endif
            importSettings.isReadable = true;
            if (!importedTextures.ContainsKey(texture.GetInstanceID()))
            {
                importedTextures.Add(texture.GetInstanceID(), importSettings);
            }
            return importSettings;
        }

        private void CheckTexturesConformity() {
			// Calculate the maximum texture count found on a _material
			int maxTextureCount = 0;
			foreach (KeyValuePair<string, List<Texture2D>> keyValue in _texturesForAtlas) {
				if (keyValue.Value.Count > 0) {
					maxTextureCount = Mathf.Max(maxTextureCount, keyValue.Value.Count);
				}
			}

			if (maxTextureCount > 0) {
				foreach (KeyValuePair<string, List<Texture2D>> keyValue in _texturesForAtlas) {
					if (keyValue.Value.Count > 0 && keyValue.Value.Count < maxTextureCount) {
						// Add the missing textures with colored one
						int numberTexturesMissing = (maxTextureCount - keyValue.Value.Count);
						for(int i=0; i < numberTexturesMissing; i++) {
							int width = _texturesForAtlas ["_MainTex"] [i].width;
							int height = _texturesForAtlas ["_MainTex"] [i].height;
							keyValue.Value.Insert(0, CreateColoredTexture2D (width, height, DefaultColoredTexture.GetDefaultTextureColor(keyValue.Key)));
                            Logger.Instance.AddLog("SuperCombiner", "Creating a colored texture " + DefaultColoredTexture.GetDefaultTextureColor(keyValue.Key) + " of size " + GetStringTextureSize(width, height) + " for " + TexturePropertyNames[keyValue.Key] + " because texture is missing.");
						}
					}
				}
			}
		}

		/// <summary>
		/// Create all the textures atlas.
		/// </summary>
		/// <param name="textureAtlasSize">Texture atlas size.</param>
		/// <param name="combineMaterials">If set to <c>true</c> combine materials.</param>
		/// <param name="name">Name.</param>
		public void PackTextures(int textureAtlasSize, int atlasPadding, bool combineMaterials, string name) {
			// Check if all materials have been contributing the same texture quantity. If not, complete with a generated default colored texture
			CheckTexturesConformity ();

			int progressBarProgression = 0;
			foreach (KeyValuePair<string, List<Texture2D>> keyValue in _texturesForAtlas) {
				if (keyValue.Value.Count > 0) {
					Texture2D packedTexture = new Texture2D (textureAtlasSize, textureAtlasSize, TextureFormat.RGBA32, false);
					packedTexture.Resize(textureAtlasSize, textureAtlasSize);
					Rect[] uvs = packedTexture.PackTextures(keyValue.Value.ToArray(), atlasPadding, textureAtlasSize);
					_packedTextures.Add (keyValue.Key, packedTexture);
					if(keyValue.Key.Equals("_MainTex")) {
                        combinedResult._combinedMaterials[_combinedIndex].uvs = uvs;
                    }
				}
#if UNITY_EDITOR
				// UI Progress bar display in Editor
				EditorUtility.DisplayProgressBar("Super Combiner", "Packing textures..." + progressBarProgression + " / " + _texturesForAtlas.Count, progressBarProgression / (float) _texturesForAtlas.Count);
#endif
				progressBarProgression++;
			}
				
			// Create the unique combined _material
			if (combineMaterials) {
				Material mat;
				if (combinedResult._originalMaterialList[_combinedIndex].Count > 0) {
					mat = new Material (combinedResult._originalMaterialList[_combinedIndex][combinedResult._originalReferenceMaterial[_combinedIndex]]._material.shader);
					mat.CopyPropertiesFromMaterial (combinedResult._originalMaterialList [_combinedIndex] [combinedResult._originalReferenceMaterial [_combinedIndex]]._material);//_combinedResult._originalMaterialList[_combinedIndex][0]._material);
				} else {
                    Logger.Instance.AddLog("SuperCombiner", "No reference _material to create the combined _material. A default standard shader _material will be created", Logger.LogLevel.LOG_ERROR);
					mat = new Material (Shader.Find("Standard"));
				}
				mat.mainTextureOffset = Vector2.zero;
				mat.mainTextureScale = Vector2.one;
				mat.color = Color.white;
				mat.name = name + "_material";

				foreach (KeyValuePair<string, Texture2D> keyValue in _packedTextures) {
					mat.SetTexture (keyValue.Key, keyValue.Value);
				}

                if (hasEmission)
                {
                    mat.SetColor("_EmissionColor", emissionColor);
                    mat.EnableKeyword("_EMISSION");
                }

                _copyedMaterials = mat;

                // Assign the new _material to result
                combinedResult.SetCombinedMaterial(mat, _combinedIndex, false);
            } 
			else 
			{
				// Create all transformed materials
				/*for (int i=0; i<_combinedResult._originalMaterialList.Count; i++) {
					Material mat = new Material (_combinedResult._originalMaterialList [i].shader);
					_copyedMaterials.Add (mat);
					_copyedMaterials [i].CopyPropertiesFromMaterial (_combinedResult._originalMaterialList [i]);

					foreach (KeyValuePair<string, Texture2D> keyValue in _packedTextures) {
						_copyedMaterials[i].SetTexture (keyValue.Key, keyValue.Value);
					}
					_copyedMaterials [i].name = name + "_" + _combinedResult._originalMaterialList [i].name;

					materialsDictionnary.Add(_copyedMaterials [i].name, _copyedMaterials [i]);
				}*/
			}
		}

        /// <summary>
        /// Saves all atlas textures in disk
        /// </summary>
        /// <param name="folder"></param>
        /// <param name="name"></param>
		public void SaveTextures(string folder, string name) {
#if UNITY_EDITOR
			foreach (KeyValuePair<string, Texture2D> keyValue in _packedTextures) {
				if (keyValue.Value != null) {
					byte[] bytes = keyValue.Value.EncodeToPNG();
					if (bytes != null) {
						string textureName = keyValue.Key;
						TexturePropertyNames.TryGetValue (keyValue.Key, out textureName);
						File.WriteAllBytes (GetTextureFilePathName(folder, name, textureName, combinedResult._combinedMaterials[_combinedIndex].displayedIndex), bytes);
					} else {
                        Logger.Instance.AddLog("SuperCombiner", "Error, please change compression mode of texture in import settings to 'none' and try combining again", Logger.LogLevel.LOG_ERROR);
					}
				}
			}
#endif
		}

        /// <summary>
        /// Returns the path + name of the atlas texture to be saved
        /// </summary>
        /// <param name="folder"></param>
        /// <param name="sessionName"></param>
        /// <param name="textureName"></param>
        /// <param name="displayedIndex"></param>
        /// <returns></returns>
        public string GetTextureFilePathName(string folder, string sessionName, string textureName, int displayedIndex)
        {
            return folder + "/Textures/" + sessionName + "_" + textureName + "_" + displayedIndex + ".png";
        }
	}
}
