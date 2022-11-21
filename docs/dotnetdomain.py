#
# This source file is part of the EdgeDB open source project.
#
# Copyright 2019-present MagicStack Inc. and the EdgeDB authors.
#
# Licensed under the Apache License, Version 2.0 (the "License");
# you may not use this file except in compliance with the License.
# You may obtain a copy of the License at
#
#     http://www.apache.org/licenses/LICENSE-2.0
#
# Unless required by applicable law or agreed to in writing, software
# distributed under the License is distributed on an "AS IS" BASIS,
# WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
# See the License for the specific language governing permissions and
# limitations under the License.
#


r"""
===========================================
:dn: domain for EdgeDB driver documentation
===========================================

"""

from __future__ import annotations

from docutils import nodes as d_nodes
from docutils.nodes import Element, Node, literal, bullet_list, list_item, field_list, Text, reference
from docutils.parsers.rst import directives  # type: ignore
import re
from sphinx.builders import Builder
from sphinx import addnodes
from sphinx.addnodes import desc_signature, pending_xref, index
from sphinx.domains.python import _pseudo_parse_arglist
from sphinx.application import Sphinx
from sphinx.domains import javascript as js
from sphinx.locale import _
from sphinx.util import docfields, logging
from sphinx.util.nodes import make_id
from sphinx.util.typing import OptionSpec
from sphinx.roles import XRefRole
from sphinx.environment import BuildEnvironment
from sphinx.directives import ObjectDescription
from sphinx import addnodes as s_nodes

from typing import Any, Dict, List, Tuple, cast, Optional

logger = logging.getLogger(__name__)

class DNFieldMixin:
    def make_xref(self, rolename, domain, target, *args, **kwargs):
        if rolename:
            return d_nodes.literal(target, target)
        return super().make_xref(rolename, domain, target, *args, **kwargs)

class DNObject(ObjectDescription[Tuple[str, str]]):
    """The .NET object for declared signature handling of directives and roles.
    
    MIT(c) edit, 2022 i0bs.
    """
    has_arguments = False
    allow_nesting = False
    option_spec: OptionSpec = {
        "no_link": directives.flag,
    }
    sig_name: str
    content_id: str

    def get_display_prefix(self) -> List[Node]:
        #: what is displayed right before the documentation entry
        return []

    def handle_signature(self, sig: str, signode: desc_signature) -> Tuple[str, str]:
        """Breaks down construct signatures

        Parses out prefix and argument list from construct definition. The
        namespace and class will be determined by the nesting of domain
        directives.
        """

        self.sig_name = sig
        
        sig = sig.strip()
        if '(' in sig and sig[-1:] == ')':
            member, arglist = sig.split('(', 1)
            member = member.strip()
            arglist = arglist[:-1].strip()
        else:
            member = sig
            arglist = None
        # If construct is nested, prefix the current prefix
        prefix = self.env.ref_context.get('dn:object', None)
        mod_name = self.env.ref_context.get('dn:module')

        name = member
        try:
            member_prefix, member_name = member.rsplit('.', 1)
        except ValueError:
            member_name = name
            member_prefix = ''
        finally:
            name = member_name
            if prefix and member_prefix:
                prefix = '.'.join([prefix, member_prefix])
            elif prefix is None and member_prefix:
                prefix = member_prefix
        fullname = name
        if prefix:
            fullname = '.'.join([prefix, name])

        signode['module'] = mod_name
        signode['object'] = prefix
        signode['fullname'] = fullname

        display_prefix = self.get_display_prefix()
        if display_prefix:
            signode += addnodes.desc_annotation('', '', *display_prefix)

        actual_prefix = None
        if prefix:
            actual_prefix = prefix
        elif mod_name:
            actual_prefix = mod_name
        if actual_prefix:
            addName = addnodes.desc_addname('', '')
            for p in actual_prefix.split('.'):
                addName += addnodes.desc_sig_name(p, p)
                addName += addnodes.desc_sig_punctuation('.', '.')
            signode += addName
        signode += addnodes.desc_name('', '', addnodes.desc_sig_name(name, name))
        if self.has_arguments:
            if not arglist:
                signode += addnodes.desc_parameterlist()
            else:
                _pseudo_parse_arglist(signode, arglist)

        
        return fullname, prefix

    def add_target_and_index(self, name_obj: Tuple[str, str], sig: str,
                             signode: desc_signature) -> None:
        """MIT(c) edit, 2022 quinchs"""
        mod_name = self.env.ref_context.get('dn:module')
        fullname = (mod_name + '.' if mod_name else '') + name_obj[0]

        dotnet_sig_id = self.create_dotnet_sig_id(fullname, sig)
        node_id = self.node_id_from_sig_id(dotnet_sig_id)

        signode['ids'].append(node_id)
        self.state.document.note_explicit_target(signode)

        domain = cast(DNDomain, self.env.get_domain('dn'))

        if not self.options.get('no_link'):
            domain.note_object(dotnet_sig_id, self.objtype, node_id, location=signode) 

        if 'noindexentry' not in self.options:
            indextext = self.get_index_text(mod_name, name_obj)
            if indextext:
                self.indexnode['entries'].append(('single', indextext, node_id, '', None))

    # Test<TType>(TType thing)
    # Test_TType-TType
    def node_id_from_sig_id(self, sig: str):
        paren_index = sig.find("(")
        product = sig[:paren_index] if paren_index != -1 else sig

        args = []
        if "(" in sig:
            args = sig.split("(")[1].split(")")[0].split(", ")
        
        # generic_match = re.match(".*?<(.*?)>\(", sig)
        # if generic_match is not None:
        #     product += "_".join(generic_match.group(1).split(", "))
        
        if any(args):
            product += "-" + "-".join(args)

        return product

    def create_dotnet_sig_id(self, fullname: str, sig: str):
        # we only want to reformat method directives, we can leave out everything else.
        if not isinstance(self, DNMethodDirective):
            return fullname

        sig = sig.strip("\n")

        args_segment = sig.split("(")[1]
        
        if args_segment[-1] != ')':
            args_segment = ":".join(args_segment.split(":")[:-1])[:-1]
        else:
            args_segment = args_segment[:-1]

        arguments = args_segment.split(", ")
       
        if len(arguments) == 0:
            return f"{fullname}()"

        # 'args' follows the format: [{this} type, name, type, name, ...], in our 
        # case we only need the type
        formatted_args = []
        for arg in arguments:
            spl = arg.strip().split(" ")
            if len(spl) == 2:
                formatted_args.append(spl[0])
            elif len(spl) == 3:
                formatted_args.append(spl[1])
            else:
                if spl[0] == "this":
                    formatted_args.append(spl[1])
                else:
                    formatted_args.append(spl[0])
        
        node_id = fullname

        node_id += f"({', '.join(formatted_args)})"

        return node_id

    def get_index_text(self, objectname: str, name_obj: Tuple[str, str]) -> str:
        name, obj = name_obj
        if self.objtype == 'function':
            if not obj:
                return _('%s() (built-in function)') % name
            return _('%s() (%s method)') % (name, obj)
        elif self.objtype == 'class':
            return _('%s() (class)') % name
        elif self.objtype == 'data':
            return _('%s (global variable or constant)') % name
        elif self.objtype == 'attribute':
            return _('%s (%s attribute)') % (name, obj)
        return ''

    def before_content(self) -> None:
        """Handle object nesting before content

        :py:class:`DNObject` represents C# .NET language constructs. For
        constructs that are nestable, this method will build up a stack of the
        nesting hierarchy so that it can be later de-nested correctly, in
        :py:meth:`after_content`.

        For constructs that aren't nestable, the stack is bypassed, and instead
        only the most recent object is tracked. This object prefix name will be
        removed with :py:meth:`after_content`.

        The following keys are used in ``self.env.ref_context``:

            dn:objects
                Stores the object prefix history. With each nested element, we
                add the object prefix to this list. When we exit that object's
                nesting level, :py:meth:`after_content` is triggered and the
                prefix is removed from the end of the list.

            dn:object
                Current object prefix. This should generally reflect the last
                element in the prefix history
        """
        prefix = None
        if self.names:
            (obj_name, obj_name_prefix) = self.names.pop()
            prefix = obj_name_prefix.strip('.') if obj_name_prefix else None
            if self.allow_nesting:
                prefix = obj_name
        if prefix:
            self.env.ref_context['dn:object'] = prefix
            if self.allow_nesting:
                objects = self.env.ref_context.setdefault('dn:objects', [])
                objects.append(prefix)

    def after_content(self) -> None:
        """Handle object de-nesting after content

        If this class is a nestable object, removing the last nested class prefix
        ends further nesting in the object.

        If this class is not a nestable object, the list of classes should not
        be altered as we didn't affect the nesting levels in
        :py:meth:`before_content`.
        """
        objects = self.env.ref_context.setdefault('dn:objects', [])
        if self.allow_nesting:
            try:
                objects.pop()
            except IndexError:
                pass
        self.env.ref_context['dn:object'] = (objects[-1] if len(objects) > 0
                                             else None)

    def make_old_id(self, fullname: str) -> str:
        """Generate old styled node_id for .NET objects.

        .. note:: Old Styled node_id was used until Sphinx-3.0.
                  This will be removed in Sphinx-5.0.
        """
        self.content_id = fullname.replace('$', '_S_')
        return self.content_id

    def transform_content(self, contentnode: addnodes.desc_content):
        """Transforms the contents of the given node annotations and
        introspectively adds xref-based targets for child methods of
        a given class.
        
        MIT(c) edit, 2022 quinchs & i0bs.
        """

        if not isinstance(self, DNClassDirective) and not isinstance(self, DNNamespaceDirective):
            return

        child_directives = contentnode.traverse(addnodes.desc_signature)

        if not len(child_directives):
            return

        if isinstance(self, DNNamespaceDirective):
            # only include types, not methods
            child_directives = [x for x in child_directives if x.parent.get("desctype") != "method"]
        
        rendered_child_elements = list(filter(lambda x: not isinstance(x, addnodes.desc), contentnode.children))
        existing_field_list = None
        idx = len(rendered_child_elements)

        while idx > 0:
            idx -= 1
            if isinstance(rendered_child_elements[idx], field_list):
                existing_field_list = rendered_child_elements[idx]
                break
            elif not isinstance(rendered_child_elements[idx], addnodes.index):
                break
            
        bullet_list_content = []

        for node in child_directives:
             # gets the nodes 'ids' property and gets the last id, 
             # incase of default ids being appended
            xref_node_id = node.get("ids")[-1]
            node_name = node.get("fullname")
            
            if isinstance(self, DNClassDirective):
                # remove the namespace/class def from full name and
                # add () to the end
                params = xref_node_id.replace(node_name, "")
                if params and params != "":
                    params = params[1:].replace("-", ", ")
                    params = f"({params})"
                else:
                    params = "()"

                node_name = node_name.split(f"{self.sig_name}.")[1] + params
            elif isinstance(self, DNNamespaceDirective):
                # remove the namspace bit from the full name
                node_name = node_name.split(f"{self.sig_name}.")[1]

            xref_node_id = xref_node_id.replace("<", "_").replace(">", "_")

            reference_node = reference('', '', literal('', node_name), internal=True, refid=xref_node_id, reftitle=node_name)

            bullet_list_content.append(list_item('', reference_node))

        if isinstance(self, DNClassDirective):
            method_field_builder = DNField('methods', label=_('Methods'), has_arg=False, bodyrolename='obj')
            method_bullet_list = bullet_list('', *bullet_list_content)
            method_field = method_field_builder.make_field([], 'dn', (None, method_bullet_list), self.env)
        
            if existing_field_list:
                existing_field_list.append(method_field)
            else:
                contentnode.append(field_list('', method_field))
        elif isinstance(self, DNNamespaceDirective):
            class_field_builder = DNField('classes', label=_('Types'), has_arg=False, bodyrolename='obj')
            class_bullet_list = bullet_list('', *bullet_list_content)
            class_field = class_field_builder.make_field([], 'dn', (None, class_bullet_list), self.env)
        
            if existing_field_list:
                existing_field_list.append(class_field)
            else:
                contentnode.append(field_list('', class_field))


class DNTypedField(DNFieldMixin, docfields.TypedField):
    pass


class DNField(DNFieldMixin, docfields.Field):
    pass


class DNGroupField(DNFieldMixin, docfields.GroupedField):
    pass


class DNCallableDirective(DNObject):
    """Represents a directive that can be callable.
    
    Example using property type field.
    
    ```
    .. dn:class:: Class

        Description of the class.

        :property string Foo: The Foo of Class.
        :property string Bar: The Bar of Class.
    ```
    """
    has_arguments = True
    doc_field_types = [  # type: ignore
        DNTypedField('properties', label=_('Properties'),
                     names=('property', 'prop'),
                     typerolename='property', typenames=('propcontrol')),
        DNTypedField('arguments', label=_('Arguments'),
                     names=('argument', 'arg', 'parameter', 'param'),
                     typerolename='function', typenames=('paramtype', 'type')),
        DNGroupField('errors', label=_('Throws'), rolename='func',
                     names=('throws', ),
                     can_collapse=True),
        DNField('returnvalue', label=_('Returns'), has_arg=False,
            names=('returns', 'return')),
        DNField('returntype', label=_('Return type'), has_arg=False,
            names=('rtype'), bodyrolename='obj')
    ]

    def handle_signature(self, sig: str, signode: desc_signature) -> Tuple[str, str]:
        # i0: adopting this code from edgedb-js as it works well for getting the
        # return type shown in the code block rendered.
        # if the function has a return type specified, clip it before
        # processing the rest of signature
        if sig[-1] != ')' and '):' in sig:
            newsig, rettype = sig.rsplit(':', 1)
            rettype = rettype.strip()
        else:
            newsig = sig
            rettype = None

        fullname, prefix = super().handle_signature(newsig, signode)

        if rettype:
            signode += s_nodes.desc_returns(rettype, rettype)

        return fullname, prefix



class DNMethodDirective(DNCallableDirective):
    """Represents a method directive.
    
    ```
    .. dn:method:: Class.Method(): ReturnType
        
        Description of the method.
        
        :param RefType Name: Description of the parameter.
        :returns: Explanation of ReturnType.
    ```
    """
    display_prefix = 'method '

class DNConstructorDirective(DNMethodDirective):
    """Represents a constructor directive.

    .. note:: 

        This is the same as DNMethodDirective. This was added for quin
        to have a display prefix difference between the two.
    """
    display_prefix = 'constructor '

class DNClassDirective(DNCallableDirective):
    """Represents a class directive.
    
    ```
    .. dn:class:: Class
        
        Description of the class.
    ```
    """
    display_prefix = 'class '
    allow_nesting = True
    option_spec = {
        **DNObject.option_spec,
        **{
            'methods': directives.flag,
        }
    }

class DNNamespaceDirective(DNObject):
    display_prefix = 'namespace '
    allow_nesting = True
    has_arguments = False
    option_spec = {
        **DNObject.option_spec,
        **{
            'error': directives.flag,
            'binary': directives.flag
        }
    }
class DNInterfaceDirective(DNClassDirective):
    """Represents an interface directive."""
    display_prefix = 'interface '

class DNStructDirective(DNClassDirective):
    """Represents a struct directive."""
    display_prefix = 'struct '

class DNEnumDirective(DNClassDirective):
    """Represents a enum directive."""
    display_prefix = 'enum '

class DNXRefRole(XRefRole):
    def update_title_and_target(self, title: str, target: str) -> Tuple[str, str]:
        title, target = super().update_title_and_target(title, target)
        if title.endswith(")()"):
            title = title[:-2]
        
        return title, target

    def process_link(self, env: BuildEnvironment, refnode: Element,
                     has_explicit_title: bool, title: str, target: str) -> Tuple[str, str]:
        # basically what sphinx.domains.python.PyXRefRole does
        refnode['dn:object'] = env.ref_context.get('dn:object')
        refnode['dn:module'] = env.ref_context.get('dn:module')
        if not has_explicit_title:
            title = title.lstrip('.')
            target = target.lstrip('~')
            if title[0:1] == '~':
                title = title[1:]
                dot = title.rfind('.')
                if dot != -1:
                    title = title[dot + 1:]
        if target[0:1] == '.':
            target = target[1:]
            refnode['refspecific'] = True
        return title, target

class DNDomain(js.JavaScriptDomain):
    name = 'dn'
    label = 'Dotnet'
    directives = {
        'function': DNCallableDirective,
        'method': DNMethodDirective,
        'constructor': DNConstructorDirective,
        'namespace': DNNamespaceDirective,
        'class': DNClassDirective,
        'interface': DNInterfaceDirective,
        'struct': DNStructDirective,
        'enum': DNEnumDirective
    }
    roles = {
        'function':  DNXRefRole(fix_parens=True),
        'method':  DNXRefRole(fix_parens=True),
        'constructor': DNXRefRole(fix_parens=True),
        'namespace': DNXRefRole(),
        'class': DNXRefRole(),
        'interface': DNXRefRole(),
        'struct': DNXRefRole(),
        'enum': DNXRefRole()
    }

    def find_obj(self, env: BuildEnvironment, mod_name: str, prefix: str, name: str,
                 typ: str, searchorder: int = 0) -> Tuple[str, Tuple[str, str, str]]:
        searches = []
        if mod_name and prefix:
            searches.append('.'.join([mod_name, prefix, name]))
        if mod_name:
            searches.append('.'.join([mod_name, name]))
        if prefix:
            searches.append('.'.join([prefix, name]))
        searches.append(name)
        
        if typ == 'method' and not typ.endswith(')'):
            searches.append(f'{name}()')

        if searchorder == 0:
            searches.reverse()

        newname = None
        for search_name in searches:
            if search_name in self.objects:
                newname = search_name

        return newname, self.objects.get(newname)

    def resolve_xref(self, env: BuildEnvironment, fromdocname: str, builder: Builder,
                     typ: str, target: str, node: pending_xref, contnode: Element
                     ) -> Optional[Element]:
        result = super().resolve_xref(env, fromdocname, builder, typ, target, node, contnode)

        if result is None:
            # generics act wierd, inner text within the ref + target form the generic str `ref_text<target>`
            old_tgt = target
            target = f"{node.astext()}<{target}>"
            result = super().resolve_xref(env, fromdocname, builder, typ, target, node, contnode)
            
            if result is None:
                logger.warning("Failed to resolve %s %s", typ, old_tgt)


        if result is not None and target.find("<") != -1 and "refid" in result:
            # reformat the ref node
            result["refid"] = result["refid"].replace("<", "_").replace(">", "_")

            if not target.endswith(")"):
                target += "()"

            result.children[0] = literal('', target)

        return result

def setup(app: Sphinx) -> Dict[str, Any]:
    app.add_domain(DNDomain)

    return {
        'version': 'builtin',
        'env_version': 2,
        'parallel_read_safe': True,
        'parallel_write_safe': True,
    }