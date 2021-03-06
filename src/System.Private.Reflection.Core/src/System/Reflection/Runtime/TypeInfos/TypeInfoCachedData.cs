// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using global::System;
using global::System.Reflection;
using global::System.Diagnostics;
using global::System.Collections.Generic;
using global::System.Reflection.Runtime.Types;
using global::System.Reflection.Runtime.General;
using global::System.Reflection.Runtime.Dispensers;
using global::System.Reflection.Runtime.FieldInfos;
using global::System.Reflection.Runtime.EventInfos;
using global::System.Reflection.Runtime.MethodInfos;
using global::System.Reflection.Runtime.PropertyInfos;

using global::Internal.Reflection.Core.NonPortable;

using global::Internal.Metadata.NativeFormat;

namespace System.Reflection.Runtime.TypeInfos
{
    //================================================================================================================
    // TypeInfoCachedData objects are allocated on-demand on a per-TypeInfo basis to cache hot data for key scenarios.
    // To maximize throughput once the cache is created, the object creates all of its internal caches up front
    // and holds entries strongly (and relying on the fact that TypeInfos themselves are held weakly to avoid immortality.)
    //
    // Note that it is possible that two threads racing to query the same TypeInfo may allocate and query two different
    // CachedData objecs. Thus, this object must not be relied upon to preserve object identity.
    //================================================================================================================
    internal sealed class TypeInfoCachedData
    {
        public TypeInfoCachedData(RuntimeTypeInfo runtimeTypeInfo)
        {
            _runtimeTypeInfo = runtimeTypeInfo;
            _methodLookupDispenser = new DispenserThatAlwaysReuses<String, RuntimeMethodInfo>(LookupDeclaredMethodByName);
            _fieldLookupDispenser = new DispenserThatAlwaysReuses<String, RuntimeFieldInfo>(LookupDeclaredFieldByName);
            _propertyLookupDispenser = new DispenserThatAlwaysReuses<String, RuntimePropertyInfo>(LookupDeclaredPropertyByName);
            _eventLookupDispenser = new DispenserThatAlwaysReuses<String, RuntimeEventInfo>(LookupDeclaredEventByName);
        }

        public RuntimeMethodInfo GetDeclaredMethod(String name)
        {
            return _methodLookupDispenser.GetOrAdd(name);
        }

        public RuntimeFieldInfo GetDeclaredField(String name)
        {
            return _fieldLookupDispenser.GetOrAdd(name);
        }

        public RuntimePropertyInfo GetDeclaredProperty(String name)
        {
            return _propertyLookupDispenser.GetOrAdd(name);
        }

        public RuntimeEventInfo GetDeclaredEvent(String name)
        {
            return _eventLookupDispenser.GetOrAdd(name);
        }


        private Dispenser<String, RuntimeMethodInfo> _methodLookupDispenser;

        private RuntimeMethodInfo LookupDeclaredMethodByName(String name)
        {
            RuntimeNamedTypeInfo definingType = _runtimeTypeInfo.AnchoringTypeDefinitionForDeclaredMembers;
            IEnumerator<RuntimeMethodInfo> matches = _runtimeTypeInfo.GetDeclaredMethodsInternal(definingType, name).GetEnumerator();
            if (!matches.MoveNext())
                return null;
            RuntimeMethodInfo result = matches.Current;
            if (matches.MoveNext())
                throw new AmbiguousMatchException();
            return result;
        }

        private Dispenser<String, RuntimeFieldInfo> _fieldLookupDispenser;

        private RuntimeFieldInfo LookupDeclaredFieldByName(String name)
        {
            RuntimeNamedTypeInfo definingType = _runtimeTypeInfo.AnchoringTypeDefinitionForDeclaredMembers;
            IEnumerator<RuntimeFieldInfo> matches = _runtimeTypeInfo.GetDeclaredFieldsInternal(definingType, name).GetEnumerator();
            if (!matches.MoveNext())
                return null;
            RuntimeFieldInfo result = matches.Current;
            if (matches.MoveNext())
                throw new AmbiguousMatchException();
            return result;
        }

        private Dispenser<String, RuntimePropertyInfo> _propertyLookupDispenser;

        private RuntimePropertyInfo LookupDeclaredPropertyByName(String name)
        {
            RuntimeNamedTypeInfo definingType = _runtimeTypeInfo.AnchoringTypeDefinitionForDeclaredMembers;
            IEnumerator<RuntimePropertyInfo> matches = _runtimeTypeInfo.GetDeclaredPropertiesInternal(definingType, name).GetEnumerator();
            if (!matches.MoveNext())
                return null;
            RuntimePropertyInfo result = matches.Current;
            if (matches.MoveNext())
                throw new AmbiguousMatchException();
            return result;
        }


        private Dispenser<String, RuntimeEventInfo> _eventLookupDispenser;

        private RuntimeEventInfo LookupDeclaredEventByName(String name)
        {
            RuntimeNamedTypeInfo definingType = _runtimeTypeInfo.AnchoringTypeDefinitionForDeclaredMembers;
            IEnumerator<RuntimeEventInfo> matches = _runtimeTypeInfo.GetDeclaredEventsInternal(definingType, name).GetEnumerator();
            if (!matches.MoveNext())
                return null;
            RuntimeEventInfo result = matches.Current;
            if (matches.MoveNext())
                throw new AmbiguousMatchException();
            return result;
        }

        private RuntimeTypeInfo _runtimeTypeInfo;
    }
}
