// The function name is a compounding of the SubtleCrypto function plus it's algorithm.
// This is because Blazor JSImport/JSExport interop won't send an object, suggest serialization for extra overhead.
// https://github.com/mdn/dom-examples/blob/main/web-crypto/encrypt-decrypt/aes-gcm.js
// https://mdn.github.io/dom-examples/web-crypto/encrypt-decrypt/index.html
export async function exportKey(format, key) {
    return {
        key: new Uint8Array(await window.crypto.subtle.exportKey(format, key))
    };
}

// https://developer.mozilla.org/en-US/docs/Web/API/RsaHashedKeyGenParams
export async function generateKey_RsaHashedKeyGen(algorithmName, algorithmModulusLength, algorithmPublicExponent, algorithmHash, extractable, keyUsages) {
    return await window.crypto.subtle.generateKey(
        {
            name: algorithmName,
            modulusLength: algorithmModulusLength,
            publicExponent: algorithmPublicExponent,
            hash: algorithmHash,
        },
        extractable,
        keyUsages);
}

// https://developer.mozilla.org/en-US/docs/Web/API/SubtleCrypto/decrypt
export async function decrypt_RsaOaep(algorithmName, privateKey, payload) {
    // Return a byte array of plain bytes
    return {
        payload: new Uint8Array(
            await window.crypto.subtle.decrypt({
                    name: algorithmName
                },
                privateKey,
                payload))
    };
}

// https://developer.mozilla.org/en-US/docs/Web/API/SubtleCrypto/encrypt
export async function encrypt_RsaOaep(algorithmName, publicKey, payload) {
    // Return object with byte array of cipher bytes
    return {
        payload: new Uint8Array(
            await window.crypto.subtle.encrypt({
                    name: algorithmName
                },
                publicKey,
                payload))
    };
}

// https://developer.mozilla.org/en-US/docs/Web/API/EcKeyGenParams
export async function generateKey_EcKeyGen(algorithmName, algorithmNamedCurve, extractable, keyUsages) {
    return await window.crypto.subtle.generateKey({
            name: algorithmName,
            namedCurve: algorithmNamedCurve,
        },
        extractable,
        keyUsages);
}

// https://developer.mozilla.org/en-US/docs/Web/API/EcdhKeyDeriveParams
export async function deriveKey_EcdhKeyDerive_AesKeyGen(algorithmEcdhName, algorithmEcdhPublic, baseKey, deriveBitLength, algorithmAesName, algorithmAesLength, extractable, keyUsages, digestAlgorithm) {
    return await window.crypto.subtle.deriveBits({
            name: algorithmEcdhName,
            public: algorithmEcdhPublic,
        },
        baseKey,
        deriveBitLength)
        .then((derivedBits) => window.crypto.subtle.digest(digestAlgorithm, derivedBits)
            .then((hashedDerivedBits) => window.crypto.subtle.importKey(
                'raw',
                hashedDerivedBits,
                {
                    name: algorithmAesName,
                    length: algorithmAesLength,
                },
                extractable,
                keyUsages
            )));
}

// https://developer.mozilla.org/en-US/docs/Web/API/EcKeyImportParams
export async function importKey_EcKeyImport(format, keyData, algorithmName, algorithmNamedCurve, extractable, keyUsages) {
    return await window.crypto.subtle.importKey(
        format,
        keyData,
        {
            name: algorithmName,
            namedCurve: algorithmNamedCurve,
        },
        extractable,
        keyUsages);
}

// https://developer.mozilla.org/en-US/docs/Web/API/SubtleCrypto/decrypt
export async function decrypt_AesGcm(algorithmName, algorithmIv, algorithmTagLength, key, payload) {
    // Return a byte array of plain bytes
    return {
        payload: new Uint8Array(
            await window.crypto.subtle.decrypt({
                    name: algorithmName,
                    iv: algorithmIv,
                    tagLength: algorithmTagLength,
                },
                key,
                payload))
    };
}

// https://developer.mozilla.org/en-US/docs/Web/API/AesGcmParams
export async function encrypt_AesGcm(algorithmName, algorithmIv, algorithmTagLength, key, payload) {
    // Return object with byte array of cipher bytes
    return {
        payload: new Uint8Array(
            await window.crypto.subtle.encrypt({
                    name: algorithmName,
                    iv: algorithmIv,
                    tagLength: algorithmTagLength,
                },
                key,
                payload))
    };
}
