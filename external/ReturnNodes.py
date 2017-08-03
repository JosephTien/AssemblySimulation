bl_info = {
    "name": "Return Nodes",
    "category": "Object",
}

import bpy
import bmesh
import sys

class ObjectMoveX(bpy.types.Operator):
    """My Object Moving Script"""      # blender will use this as a tooltip for menu items and buttons.
    bl_idname = "object.return_nodes"        # unique identifier for buttons and menu items to reference.
    bl_label = "Return Nodes"         # display name in the interface.
    bl_options = {'REGISTER', 'UNDO'}  # enable undo for the operator.

    def execute(self, context):
        file = open('C:\\target\\thinstruct.txt','w')
        id = 0
        nodeList = []
        lineList = []
        map = dict()
        obj = bpy.context.object
        if obj.mode == 'EDIT':
            bm = bmesh.from_edit_mesh(obj.data)
            for e in bm.edges:
                if e.select:
                    n0 = e.verts[0]
                    n1 = e.verts[1]
                    if(n0 not in map):
                        map[n0] = id
                        id = id + 1
                        nodeList.append(n0)
                    if(n1 not in map):
                        map[n1] = id
                        id = id + 1
                        nodeList.append(n1)
                    lineList.append((map[n0],map[n1]))
            file.write(str(len(nodeList)) + " " + str(len(lineList)) + "\n")
            for n in nodeList:
                file.write('%f %f %f\n' % (n.co.x, n.co.y, n.co.z))
            for l in lineList:
                file.write('%d %d\n' % (l[0], l[1]))
            file.close()
                    
                    
        return {'FINISHED'}            # this lets blender know the operator finished successfully.

def register():
    bpy.utils.register_class(ObjectMoveX)


def unregister():
    bpy.utils.unregister_class(ObjectMoveX)


# This allows you to run the script directly from blenders text editor
# to test the addon without having to install it.
if __name__ == "__main__":
    register()